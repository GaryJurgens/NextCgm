# How to Get It Running in Production (VPS)

This guide explains how to publish your NextCgm application to a Docker registry (like Docker Hub) and deploy it to a Linux Virtual Private Server (VPS). 

By publishing your application as a Docker image, you avoid having to copy your entire source code repository to your production server. The VPS only needs the `docker-compose.prod.yml` file to pull your pre-built image and spin everything up.

---

## Step 1: Create a Docker Hub Account
If you don't already have one, go to [hub.docker.com](https://hub.docker.com/) and create a free account. Make sure to note your **username**.

## Step 2: Update the Compose File
Open `C:\GitHub_Repos\NextCgm\docker-compose.prod.yml` on your local Windows machine.
Find line 21 and replace `yourdockerhubusername` with your actual Docker Hub username:

```yaml
  nextcgm-backend:
    # Replace "yourdockerhubusername" with your actual Docker Hub username
    image: yourdockerhubusername/nextcgm-backend:latest
```

## Step 3: Build and Push from your Windows Machine
Open a terminal (PowerShell or Command Prompt) in `C:\GitHub_Repos\NextCgm` and run the following commands:

1. **Login to Docker Hub:**
   ```bash
   docker login
   ```
   *(Enter your Docker Hub username and password when prompted)*

2. **Build the Image:**
   ```bash
to build the new image
docker-compose build nextcgm-backend

   docker-compose -f docker-compose.prod.yml build

for local build use
docker-compose -f docker-compose-local.yml build

then copy dcoker-compose.yml to vpn root


use 
   ```
   *(This compiles your .NET application and tags it with your Docker Hub username)*

3. **Push the Image to the Cloud:**
   ```bash

this updates the image, 

   docker-compose -f docker-compose-local.yml push

but you need to copy the docker-compose.yml file to the VPN, as its different, espicly regarading file and build parths and instrations
   ```
   *(This uploads your compiled application image to Docker Hub so your VPS can download it later)*

---

## Step 4: Prepare the VPS

stop all containers

docker stop $(docker ps -aq)

remove all containers 
docker rm $(docker ps -aq)

To see logs for the Backend:
docker logs nextcgm-backend

To see logs for the UI:
docker logs nginx-ui


Now, your VPS doesn't need your source code at all! It only needs the `docker-compose.prod.yml` file.

1. Copy **only** the `docker-compose.prod.yml` file from your local machine to a folder on your VPS (e.g., `/opt/nextcgm/`).
2. Open the file on your VPS using a text editor (like `nano` or `vim`) and **delete** the three lines under `build:` (lines 22-24) so it looks exactly like this:

```yaml
  nextcgm-backend:
    # Replace "yourdockerhubusername" with your actual Docker Hub username
    image: yourdockerhubusername/nextcgm-backend:latest
    container_name: nextcgm-backend
    # Run as root so the container can access the mounted docker.sock
    user: root
```
*(Removing the `build` section tells Docker Compose to pull the image from Docker Hub instead of trying to build it from local source code).*

---

## Step 5: Configure Cloudflare DNS Automation
The application is fully equipped to automatically create DNS records for every new container via the Cloudflare API. To enable this in production:

1. Log into your Cloudflare dashboard and generate an **API Token** with permissions to edit DNS records for your zone.
2. Find your **Zone ID** on the overview page for your domain.
3. Open your `appsettings.json` file (or pass these via environment variables in your `docker-compose.prod.yml`) and update the `CloudflareSettings` block:

```json
"CloudflareSettings": {
    "ApiBaseUrl": "https://api.cloudflare.com/client/v4/",
    "ApiToken": "YOUR_CLOUDFLARE_API_TOKEN",
    "ZoneId": "YOUR_CLOUDFLARE_ZONE_ID",
    "Domain": "nextcgm.co.za",
    "TargetIp": "YOUR_VPS_IP_ADDRESS"
}
```
*Ensure `TargetIp` is set to the public IP address of your VPS so the new subdomains route correctly to your server.*

---

## Step 6: Run it on your VPS
1. SSH into your VPS and navigate to the folder where you placed the `docker-compose.prod.yml` file.
2. Run this single command to start the entire stack:
 foce docker to pull latest image

docker-compose pull


docker-compose up -d


if containers are stuck, ie container error

 docker rm -f nextcgm-backend nginx-ui nextcgm-backend or just // docker rm -f nextcgm-backend nginx-ui

prune Branches ro remove stuck conainers


docker system prune

then try this

docker compose up -d


```bash
docker-compose -f docker-compose.prod.yml up -d --build   / docker-compose -f docker-compose.yml up -d --build
```

Docker on your VPS will automatically:
- Download your pre-built .NET backend image from Docker Hub.
- Download the Nginx UI image.
- Create the `nextcgm_network`.
- Spin everything up instantly and connect them together!

---

## Ngix for Api.nextcgm.co.za

Add site 

server api.nextcgm.com

location 

proxy_pass http://nextcgm-backend:8080;
include proxy_params;
proxy_set_header Host $host;
proxy_set_header X-Real-IP $remote_addr;
proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
proxy_set_header X-Forwarded-Proto $scheme;


if ssl step gives problems

location must be a /

then this must be pasted, it removes a path

proxy_pass http://nextcgm-backend:8080;
proxy_set_header Host $host;
proxy_set_header X-Real-IP $remote_addr;
proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
proxy_set_header X-Forwarded-Proto $scheme;


## ⚠️ Important Production Notes
- **Docker Socket Security:** Mounting `/var/run/docker.sock` gives the container full control over the host's Docker daemon. Ensure your VPS is properly secured and only accessible via SSH keys.
- **Nginx UI Default Login:** When you first access Nginx UI on your VPS (via port 80 or 443), it will prompt you to set up an admin account.
- **Environment Variables:** The `docker-compose.prod.yml` file automatically sets `DockerSettings__IsLocalDevelopment=false`. This ensures the .NET app uses Docker's internal DNS (`http://<container-name>:1337`) instead of `host.docker.internal` when routing traffic, which is the correct behavior for Linux production environments.