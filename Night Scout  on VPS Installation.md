**This guide outlines the professional deployment of a Nightscout instance using Docker, Nginx UI for graphical management, and Cloudflare for DNS and SSL security.**



**Phase 1: Domain and DNS Configuration**



* Before configuring the server, ensure your domain routing is established.



* **Register Domain:** Register your desired domain via Domains.co.za (or your preferred registrar).



**Setup Cloudflare:**



* Create a free account on Cloudflare.



* Add your site and note the Cloudflare Nameservers provided.



* **Update Nameservers:** Back in the Domains.co.za portal, replace the existing nameservers with the Cloudflare nameservers.



* **Note:** DNS propagation can take anywhere from 1 to 24 hours.



* **Configure DNS Record:** In the Cloudflare Dashboard, go to DNS > Records and add an A Record:



* **Name:** cgm (or your preferred subdomain).



* **IPv4 address:** Your VPS IP address.



* **Proxy status:** Set to Proxied (Orange Cloud ON).



**Phase 2: VPS Provisioning and Docker Setup**



Deploy a Virtual Private Server (VPS) via Absolute Hosting (or similar) using Ubuntu (Latest LTS).



* Access the Server

&#x09;Connect via SSH (using PuTTY or Terminal): use server Ip Address, then set Username (normally root) and password that you used on server setup.

* &#x20;You might need to Setup a filewall rule in your host for port 22 if blocked, origin 0.0.0.0/0 (for everywhere or just your Computes IP Address)  TCP Protical.
* Destination your Servers IP and also port 22.





**Install Docker Engine**

Run the following commands to install the Docker runtime:





**Update system and install dependencies**

sudo apt update \&\& sudo apt install -y ca-certificates curl gnupg



**Add Docker’s official GPG key**

sudo install -m 0755 -d /etc/apt/keyrings

curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg

sudo chmod a+r /etc/apt/keyrings/docker.gpg



**Set up the repository**

echo "deb \[arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu $(. /etc/os-release \&\& echo "$VERSION\_CODENAME") stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null



**Install Docker and Docker Compose**

sudo apt update \&\& sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin docker-compose



**Phase 3: Install Nginx UI (GUI Manager)**



Instead of editing config files manually, we will use Nginx UI to manage our reverse proxy.



* Deploy the Nginx UI Container:



&#x09;run the Docker Command Below



\-----------------------------------------------



docker run -d \\

&#x20; --name nginx-ui \\

&#x20; --restart always \\

&#x20; --network host \\

&#x20; -v /etc/nginx:/etc/nginx \\

&#x20; -v /etc/nginx-ui:/etc/nginx-ui \\

&#x20; uozi/nginx-ui:latest



\----------------------------------------------



**Firewall Configuration:**



**Ensure your VPS firewall (UFW or Cloud Dashboard) allows the following traffic:**



**TCP = Transport Communication Protocol**



* 80/TCP (HTTP)



* 443/TCP (HTTPS)



* 9000/TCP (Nginx UI Dashboard)



**Initial Login:**



* Navigate to http://your-server-ip:9000.



* Default Credentials: admin / admin.



* Action: Change these credentials immediately under account settings.



**Phase 4: SSL Certificate Setup**



**Generate Origin Certificate:** In the Cloudflare Dashboard, go to SSL/TLS > Origin Server and click Create Certificate.



**NOTE:** the Certificates and Key path must be the same Except the name of the file "NextCgm.crt" and "NextCgm.key" must change to meet the doing you are using or proxying



**Import to Nginx UI:**



* Go to Certificates > Import.



* Name: Cloudflare-Origin



* Certificate Path: /etc/nginx/nextcgm.crt



* Key Path: /etc/nginx/nextcgm.key



* Paste the Certificate and Private Key content from Cloudflare into the respective boxes and save.





**Phase 5: Deploy Nightscout Container**



If using Azure Cosmos DB RU (MongoDB API), ensure you have your connection string ready.



Copy it , it normaly under connection string, copy the long key that is dotted.



**Note : change the port Number as described above**



**Change the Name of the Container if using multiple dockers**



**If your Using Azure Cosmod DB for Mongo (RU)**



* Add the new Database to the Cluster (if exists) , call it like nightscout-UserIdentitication.



**Get the Connection string from the left bar, use the full key, you have to diplay it and copy it.**



**Modfy the connection string as follows.**



* 10255/nightscout (your databse Name) - so it looks for the new database.



**Options**



API\_Secret - this must be a lowercase string that devices identify and secure your site



run this Docker Command, look at the Database URL, make the changes to the One you copied regarding night scout in the database and container name.



change the -p port number to something unique for the Reverse proxy, if you are going to running multiple nightscout instances.



API\_SECRET="adolfieoros450450" - change the Api Key to a series of locatercase words and numbers no special char, you will need this in you Xdrip and to login into the website as an administrator.



If you want to try adding the second domain again later:



keep these two **"Golden Rules"** in mind for the next attempt:



**Unique Host Ports:** Your first instance uses 1337:1337. Your **second instance must use something else on the left side**, like **1338:1337**.



**Unique Container Names**: You can't have two containers named nightscout. Name them nightscout-gary and nightscout-next



\----------------------------------------------------------------------------------

sudo docker run -d \\

&#x20; --name **nightscout** \\

&#x20; --restart always \\

&#x20; -p **1337:1337** \\

&#x20; -e INSECURE\_USE\_HTTP=true \\

&#x20; -e **API\_SECRET="adolfieoros450450"** \\

&#x20; -e MONGO\_CONNECTION="mongodb://nightscoutcgm:C7NLstoHoeTC6c5KPef2Wd2Zfq0YRd2MCj7Yvy8jSzPxvtO15rxofBqLtNaldK5X498BGnAdqcTQACDbrowASQ%3D%3D@nightscoutcgm.mongo.cosmos.azure.com:10255/**nightscout**?ssl=true\&appName=%40nightscoutcgm%40\&retryWrites=false\&replicaSet=globaldb\&readPreference=primary\&maxIdleTimeMS=120000\&connectTimeoutMS=10000\&authSource=admin\&authMechanism=SCRAM-SHA-1

" \\

&#x20; -e DISPLAY\_UNITS="mmol/L" \\

&#x20; -e ALARM\_TYPES="none" \\

&#x20; -e ALARM\_URGENT\_HIGH="off" \\

&#x20; -e ALARM\_HIGH="off" \\

&#x20; -e ALARM\_LOW="off" \\

&#x20; -e ALARM\_URGENT\_LOW="off" \\

&#x20; -e ALARM\_TIMEAGO\_WARN="off" \\

&#x20; -e ALARM\_TIMEAGO\_URGENT="off" \\

&#x20; -e SHOW\_FORECAST="true" \\

&#x20; -e DBSIZE\_MAX=20000\\

&#x20; -e DBSIZE\_IN\_MB=true\\

&#x20; nightscout/cgm-remote-monitor:latest



\-------------------------------------------------------------------------------------



**Phase 6: Configure Reverse Proxy (Nginx UI)**



**To link your domain name to the Docker container:**



* Go to Manage Sites > Add Site.



* Server Name: cgm.yourdomain.com



* Advanced Settings (Directives):



* Ensure Listen is set to 80 and 443 ssl.

&#x20;listen 443 ssl



&#x20;ssl\_certificate /etc/nginx/cloudflare.crt

&#x20;ssl\_certificate\_key /etc/nginx/cloudflare.key (these are to your new Domin Certs)

&#x20;proxy\_hide\_header 'Access-Control-Allow-Origin'

&#x20;add\_header 'Access-Control-Allow-Origin' '\*'

&#x20;add\_header Access-Control-Allow-Headers' '\*'





* Select your Cloudflare-Origin certificate.



* Location Block: Inside the location " /" then select it and you will a black box block, paste the following:





Notice the 1337 , change this to the custom one you used in your docker install

\-----------------------------------------

proxy\_pass http://127.0.0.1:1337;

proxy\_set\_header Host $host;

proxy\_set\_header X-Real-IP $remote\_addr;

proxy\_set\_header Upgrade $http\_upgrade;

proxy\_set\_header Connection "upgrade";



**-----------------------------------------**



**Save and Reload: Click save.** Nginx UI will automatically test and reload the configuration.



**Cloudflare SSL Settings:** In the Cloudflare dashboard, set SSL/TLS encryption mode to Full (Strict)/ flexible also works



**Test Site:** Navigate to https://cgm.yourdomain.com. You should see the Nightscout landing page secured by Cloudflare.









**Important - Backfilling Events faster on X Drip, we need to turn of the GUI ping Process on the server.**





Here is the "Ubuntu way" to find it, stop it, and get that backfill moving. - will fill extremely fast



**​1. Find the "Hidden" Process**



​Run this command to see every process that has "night" or "node" in the name:



**ps aux | grep -E 'node|night'**



You will see a list. Look for the line that mentions server.js or start. The second column is the PID (Process ID).

​

**2. Kill the Process (To clear the path)**



​Once you find the PID (e.g., 1234), stop it manually:



***kill -9 1234***



Note: If it's a Docker container,



&#x20;***sudo docker ps***



will show it. Stop it with



**sudo docker stop <container\_name>.**



restart docker container





***sudo docker start <container\_name>***



***----------------------------------------------------------------------------------------***



**DO NOT USE**





**docker run:** Used to create and start a brand new container from an image. If you use run instead of start, you’ll end up with a duplicate container and will likely run into port conflict errors (since the old container still "occupies" that port configuration).



\-------------------------------------------------------------------------------------------



**docker restart:** A shortcut that stops and then immediately starts the container again. This is useful if you’ve changed a configuration file and need the service to reload.







If you can't remember the exact name of your stopped container, you can find it by running:





**sudo docker ps -a**





The -a (all) flag is necessary because a standard docker ps only shows containers that are currently running.







.

