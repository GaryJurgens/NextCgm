# Traefik & Cloudflare Tunnels Overview

## What is Traefik?
Traefik is a modern, dynamic reverse proxy and load balancer designed specifically for microservices and containerized environments. It automatically discovers the right configuration for your services by listening directly to your infrastructure (like Docker) in real-time. This completely replaces traditional static proxies like Nginx, eliminating the need to manually write or reload configuration files.

## How We Use Traefik
We use Traefik as our primary internal router to direct incoming web traffic to the correct Docker containers based on the requested domain name (Host header). It automatically detects when new containers start or stop and creates the routing rules on the fly. 

## What is Cloudflare Tunnels?
Cloudflare Tunnels provides a secure, outbound-only connection between your server and the Cloudflare network. Because the connection is initiated from inside your server outwards, you do not need to open any inbound ports on your firewall. It protects your infrastructure from direct attacks by ensuring all traffic is routed securely through Cloudflare's edge.

## How We Use Cloudflare Tunnels with Traefik
We use Cloudflare Tunnels to securely bridge the public internet to our server, capturing all wildcard subdomain traffic (`*.nextcgm.co.za`) at the edge. The tunnel then forwards this traffic internally directly to Traefik, which handles the final routing to the specific backend containers.

## The Cloudflare Daemon (`cloudflared`)
The `cloudflared` daemon is a lightweight Docker container running on our server that establishes the secure outbound connection to Cloudflare using a unique tunnel token. It operates in "Remote Management" mode, meaning all routing rules are managed in the Cloudflare Zero Trust dashboard rather than local configuration files.

## Marking Docker Containers for Auto-Routing
To make Traefik automatically route traffic to a new container, we simply attach specific Docker labels to the container during creation. The label `traefik.http.routers.<name>.rule=Host('<domain>')` is a Traefik-specific syntax where `<name>` is a unique identifier for the router, and `Host` tells Traefik to match the incoming HTTP Host header to the specified domain. By adding these labels, Traefik instantly detects the container and creates the routing rules without any manual intervention.

**Example:**
If we create a new container for a user named "Dolfie", we inject the following labels:
- `traefik.enable=true` (Tells Traefik to expose this container)
- `traefik.http.routers.dolfie.rule=Host('dolfie.nextcgm.co.za')` (Tells Traefik to route any traffic requesting `dolfie.nextcgm.co.za` to this container)
- `traefik.http.services.dolfie.loadbalancer.server.port=80` (Tells Traefik the container is listening internally on port 80)