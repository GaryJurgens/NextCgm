using Medo;
using NextCgm.DataEntities.Containers;

namespace NextCgm.DataEntities.Nginx
{
    public class NginxRoutingRule
    {
        public Guid NginxRoutingRuleID { get; set; } = Uuid7.NewGuid();
        public Guid DockerContainerID { get; set; }
        public virtual DockerContainers Container { get; set; }

        // --- External Facing ---
        public string ExternalHost { get; set; } // e.g., "alpha.myapp.com"
        public int ListenPort { get; set; }      // e.g., 80 or 443

        // --- Internal NGINX -> Docker Routing ---
        public int TargetContainerPort { get; set; } // The port INSIDE the container (e.g., 5000)

        // NGINX Unit "Pass" string: "applications/Cat-Alpha-App"
        public string NginxPassPath => $"applications/{Container.AppUniqueName}";
    }
}
