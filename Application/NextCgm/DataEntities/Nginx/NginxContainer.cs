using Medo;
using NextCgm.DataEntities.Containers;

namespace NextCgm.DataEntities.Nginx
{
    public class NginxContainer
    {

        public Guid NginxContainerID { get; set; } = Uuid7.NewGuid();

        // Relationship to your Docker Container
        public Guid DockerContainerID { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.ForeignKey("DockerContainerID")]
        public virtual DockerContainers Container { get; set; }

        // --- Routing Information ---
        // This is what NGINX uses to decide which request goes where
        public string Hostname { get; set; } = string.Empty;
        public string PathPrefix { get; set; } = string.Empty;
        public int ExternalPort { get; set; }
        public string InternalAddress { get; set; } = string.Empty;
        public int InternalPort { get; set; }
        public bool EnableWebSockets { get; set; }
        public int ClientMaxBodySizeMb { get; set; } = 10;
        public string SslCertName { get; set; } = string.Empty;
        public string SslCertKey { get; set; } = string.Empty;
    }
}
