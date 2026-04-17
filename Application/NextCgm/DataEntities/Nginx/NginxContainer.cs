using Medo;
using NextCgm.DataEntities.Containers;

namespace NextCgm.DataEntities.Nginx
{
    public class NginxContainer
    {

        public Guid NginxContainerID { get; set; } = Uuid7.NewGuid();

        // Relationship to your Docker Container
        public Guid DockerContainerID { get; set; }
        public virtual DockerContainers Container { get; set; }

        // --- Routing Information ---
        // This is what NGINX uses to decide which request goes where
        public string Hostname { get; set; }     // e.g. "cat-alpha.com"
        public string PathPrefix { get; set; }   // e.g. "/"
        public int ExternalPort { get; set; }    // e.g. 80

        // --- Backend Information ---
        // How NGINX finds the Docker container
        public string InternalAddress { get; set; } // Docker Name or Container IP
        public int InternalPort { get; set; }       // The port the C# app uses inside Docker

        // --- NGINX Policy Settings ---
        public bool EnableWebSockets { get; set; }  // Required if your C# app uses SignalR
        public int ClientMaxBodySizeMb { get; set; } = 10;
        public string SslCertName { get; set; }
        public string SslCertKey { get; set; }
    }
}
