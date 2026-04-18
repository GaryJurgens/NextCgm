# NextCgm Project - Setup & Architecture Guide

This document summarizes the architecture, recent fixes, and setup instructions for the NextCgm automated container provisioning system.

## 🏗️ Architecture Overview

NextCgm is a .NET 10 application designed to automatically provision Docker containers (specifically `nightscout/cgm-remote-monitor`) for users and dynamically configure an Nginx reverse proxy using Nginx UI.

### Key Components:
1. **.NET Backend (`NextCgm`)**: Handles user authentication, database management (PostgreSQL via EF Core), and Docker API interactions.
2. **Vue.js Frontend (`Portal`)**: A web interface for users to log in and click "Create New Container".
3. **Docker Daemon**: The host Docker engine that the .NET app communicates with to pull images and spin up new containers.
4. **Nginx UI (`nginx-ui`)**: A separate Docker container that acts as the reverse proxy. The .NET app injects configuration files and SQLite database records directly into Nginx UI so that new containers are instantly routed and visible in the UI.

---

## 🛠️ Recent Fixes & Improvements

During our development session, we resolved several critical issues to make the end-to-end flow work:

1. **500 Internal Server Error (Authentication Middleware)**
   - **Issue:** The `CreateContainerEndpoint` required authentication, but the JWT authentication middleware was missing from `Program.cs`. This caused FastEndpoints to throw an internal exception before the breakpoint was even hit.
   - **Fix:** Added `AddAuthenticationJwtBearer()`, `AddAuthorization()`, `UseAuthentication()`, and `UseAuthorization()` to `Program.cs`.

2. **EF Core Database Crashes (`DbUpdateException`)**
   - **Issue:** The project uses `<Nullable>enable</Nullable>`. Several string properties in the data entities (`DockerLogger`, `DockerContainers`, etc.) were not initialized, causing EF Core to reject them as `NULL`. Additionally, there was a foreign key mismatch on `NginxContainer` and `RoutingRule`.
   - **Fix:** Initialized all non-nullable strings to `string.Empty`, added `[ForeignKey("DockerContainerID")]` attributes to navigation properties, and applied a new database migration (`FixNginxForeignKeys`).

3. **Docker Port Binding Error (`Cannot assign requested address`)**
   - **Issue:** The app was checking the *container's* internal network for free ports instead of the *host's* network, leading to port collisions (e.g., trying to bind port 8000 when it was already in use).
   - **Fix:** Updated `GeneratePortInRange` to query the PostgreSQL database for previously assigned ports and pick a random available port between `10000` and `20000`.

4. **Docker Network Isolation (`502 Bad Gateway`)**
   - **Issue:** Nginx UI and the newly created Nightscout containers were on different Docker networks, so Nginx couldn't route traffic to them using their internal container IDs.
   - **Fix:** Configured Nginx to route traffic through the host machine using `host.docker.internal:<ExposedPort>`. This guarantees connectivity regardless of Docker network isolation.

5. **Nginx UI "Sites" Tab Integration**
   - **Issue:** Nginx UI uses an internal SQLite database (`app.db`) to track sites. Manually dropping `.conf` files into the folder didn't make them appear in the "Sites" tab.
   - **Fix:** Reverse-engineered the Nginx UI SQLite schema. The .NET app now automatically:
     1. Writes the config to `sites-available`.
     2. Creates a symlink in `sites-enabled`.
     3. Inserts a record directly into the Nginx UI SQLite database.
     4. Reloads the Nginx container via the Docker API.

---

## 🚀 How to Get It Running

To run the full stack locally on Windows with Docker Desktop, follow these steps:

### 1. Start Nginx UI
Make sure your `docker-compose.yml` for Nginx UI is running and has the correct volume mappings so the .NET app can share files with it.

```yaml
services:
  nginx-ui:
    image: uozi/nginx-ui:latest
    container_name: nginx-ui
    ports:
      - "80:80"
      - "443:443"
    volumes:
      # This maps the host folder to Nginx UI
      - ./nginx:/etc/nginx
      - ./nginx-ui:/etc/nginx-ui
      # Mount the Docker socket so Nginx UI can interact with Docker if needed
      - /var/run/docker.sock:/var/run/docker.sock
    environment:
      - NGINX_UI_OFFICIAL_DOCKER=true
    restart: always
```
Run `docker-compose up -d` in the folder containing this file (e.g., `C:\Docker\nginx-ui`).

### 2. Configure Visual Studio Launch Settings
In Visual Studio, ensure your `launchSettings.json` (under `Properties`) has the following `DockerfileRunArguments` for the "Container (Dockerfile)" profile. This is **critical** because it mounts the Docker socket and the shared Nginx folders into the .NET container:

```json
"DockerfileRunArguments": "-v /var/run/docker.sock:/var/run/docker.sock -v C:/Docker/nginx-ui:/app/nginx_data -u root"
```
*(Note: Adjust `C:/Docker/nginx-ui` if your Nginx UI compose file is located somewhere else).*

### 3. Run the .NET Backend
1. Open the `NextCgm` solution in Visual Studio.
2. Select the **"Container (Dockerfile)"** launch profile from the debug dropdown.
3. Click **Start Debugging (F5)**. The .NET app will spin up inside a Linux container as the `root` user (giving it access to the Docker socket).

### 4. Run the Vue.js Frontend
1. Open a terminal and navigate to `C:\GitHub_Repos\NextCgm\Application\Portal`.
2. Run `npm install` (if you haven't already).
3. Run `npm run dev`.
4. Open your browser to `http://localhost:5174` (or whatever port Vite assigns).

### 5. Test the Flow
1. Register/Login to the Vue.js portal.
2. Click **"Create New Container"**.
3. Wait a few seconds. The container will appear in your dashboard.
4. Open **Nginx UI** (`http://localhost`). Go to the **Sites** tab, and you will see your newly generated domain (e.g., `honest-whiskey.nextcgm.co.za`) listed and Enabled!

### 6. Local DNS Testing
To test the actual domain name locally:
1. Open Notepad as Administrator.
2. Edit `C:\Windows\System32\drivers\etc\hosts`.
3. Add a line at the bottom: `127.0.0.1 your-generated-domain.nextcgm.co.za`
4. Type that domain into your browser, and Nginx will route you directly to your new Nightscout container!

### 7. Production Deployment (VPS)
When deploying to a Linux VPS (instead of Docker Desktop), you should use Docker's internal DNS to route traffic directly between containers without exposing ports on the host.

1. Open `appsettings.json` and set `"IsLocalDevelopment": false`.
2. Ensure all your containers (NextCgm, nginx-ui, and the generated Nightscout containers) are on the same Docker network.
3. Create a Docker network on your VPS:
   ```bash
   docker network create nextcgm_network
   ```
4. Update your `docker-compose.yml` files to attach `NextCgm` and `nginx-ui` to this network:
   ```yaml
   networks:
     default:
       name: nextcgm_network
       external: true
   ```
5. When `IsLocalDevelopment` is false, the .NET app will automatically attach new Nightscout containers to `nextcgm_network` and configure Nginx to route traffic directly to the container's internal IP using its hostname, completely bypassing the host network!