# Migrating to Traefik & Cloudflare Tunnels

This document outlines the steps taken to migrate the NextCgm architecture from Nginx Proxy Manager and dynamic Cloudflare API DNS records to a streamlined **Traefik + Cloudflare Tunnels** setup.

## The Goal
To simplify dynamic routing for Docker containers by replacing manual Nginx configurations and Cloudflare API calls with Traefik's automatic label detection and a single Cloudflare Tunnel.

## The New Architecture
1. **Cloudflare DNS:** A single wildcard CNAME record (`*.nextcgm.co.za`) points all traffic to the Cloudflare Tunnel.
2. **Cloudflare Tunnel (`cloudflared`):** Runs in "Remote Management" mode and securely forwards all incoming traffic to Traefik's internal port 80.
3. **Traefik Reverse Proxy:** Listens to the Docker socket. When a new container is spun up with specific Traefik labels, Traefik instantly creates a route for it based on the `Host` header.
4. **Dynamic Containers:** The C# backend creates Docker containers and simply attaches Traefik labels to them. No external API calls are needed.

---

## Step-by-Step Implementation Guide

### 1. Docker Compose Configuration
We replaced `nginx-proxy-manager` with `traefik:latest` and `cloudflare/cloudflared:latest`. 

**Key Discoveries:**
* **Traefik Version Bug:** Traefik `v3.0` had a hardcoded Docker API version (`1.24`) which caused it to fail on newer Docker engines (v26.0+) with the error: `client version 1.24 is too old. Minimum supported API version is 1.40`. 
* **The Fix:** We upgraded to `traefik:latest` (v3.1+) and explicitly set the environment variable `DOCKER_API_VERSION=1.41` to ensure compatibility with the host's Docker daemon.

### 2. Securing the Traefik Dashboard
We wanted the Traefik dashboard to be accessible via `traefik.nextcgm.co.za` but secured with Basic Authentication.

**Key Discoveries:**
* **Secure Mode:** We removed `--api.insecure=true` and used `--api.dashboard=true`.
* **Basic Auth Hash:** Traefik requires passwords to be hashed (MD5, SHA1, or BCrypt). Plain text passwords (like `Admin:myPassword`) will break the router. We generated a BCrypt hash and escaped the `$` symbols in the `docker-compose.yml` by doubling them (`$$`).
* **Trailing Slash:** Traefik is extremely strict about the dashboard URL. You **MUST** include the trailing slash: `https://traefik.nextcgm.co.za/dashboard/`. Without it, Traefik returns a `404 page not found`.

### 3. Cloudflare Zero Trust Configuration (Remote Management)
We opted for "Remote Management" mode, meaning no local `config.yml` or credentials files are needed on the server. The `cloudflared` container authenticates using only the `TUNNEL_TOKEN` environment variable.

**Key Discoveries:**
* **Published Application Routes:** In the Cloudflare Zero Trust dashboard (under Tunnels -> Configure -> Published application routes), we created a route to catch all traffic.
  * **Subdomain:** `*` (Wildcard)
  * **Domain:** `nextcgm.co.za`
  * **Path:** *(Must be completely empty! Placeholder text like `^/blog` should be ignored/cleared).*
  * **Service Type:** `HTTP`
  * **URL:** `traefik:80` (Since both containers are on the same Docker network, Cloudflared routes directly to Traefik's internal port 80).

### 4. Cloudflare DNS Configuration
When you create a wildcard route (`*`) in Zero Trust, Cloudflare warns you: *"This domain contains a wildcard, so no DNS record will be created."*

**Key Discoveries:**
* **Manual CNAME Creation:** You must manually go to your Cloudflare DNS settings and create a `CNAME` record for `*`.
* **Target Mismatch (Error 1016):** If you get an `Error 1016: Origin DNS error`, it means your CNAME record is pointing to an old or incorrect Tunnel UUID. The Target must exactly match your current active tunnel: `<your-active-tunnel-uuid>.cfargotunnel.com`.

### 5. C# Backend Refactoring
With Traefik handling routing dynamically based on Docker labels, we were able to completely strip out the old Nginx and Cloudflare API logic.

**Key Discoveries:**
* **Port Conflicts:** We moved the backend API from port `8080` to `5000` to ensure it didn't conflict with Traefik's default dashboard port or the dynamic container port range (`10000` to `20000`).
* **Dynamic Labels:** When the C# backend creates a new Docker container (`CreateContainerParameters`), it now simply injects these labels:
  ```csharp
  Labels = new Dictionary<string, string>
  {
      { "traefik.enable", "true" },
      { $"traefik.http.routers.{SubDomainGen}.rule", $"Host(`{SubDomainGen}{_options.EndDomain}`)" },
      { $"traefik.http.services.{SubDomainGen}.loadbalancer.server.port", _options.ContainerPort.ToString() }
  }
  ```
* **Code Cleanup:** We successfully deleted `NginxService`, `CloudflareService`, their respective endpoints, DTOs, and Entity Framework DbSets, massively simplifying the codebase.

---

## Conclusion
The new architecture is highly resilient. Cloudflare handles DDoS protection and SSL termination at the edge, securely tunneling traffic to the server without exposing any public IP addresses. Traefik listens internally and dynamically routes traffic to containers the moment they spin up, requiring zero manual configuration or API calls!