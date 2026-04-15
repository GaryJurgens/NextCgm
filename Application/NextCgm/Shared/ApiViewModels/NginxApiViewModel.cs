using System;

namespace NextCgm.Shared.ApiViewModels
{
    public class NginxApiViewModel
    {
        public Guid NginxContainerID { get; set; }
        public Guid DockerContainerID { get; set; }
        public string Hostname { get; set; } = string.Empty;
        public string PathPrefix { get; set; } = string.Empty;
        public int ExternalPort { get; set; }
        public string InternalAddress { get; set; } = string.Empty;
        public int InternalPort { get; set; }
        public bool EnableWebSockets { get; set; }
        public int ClientMaxBodySizeMb { get; set; }
        public string SslCertName { get; set; } = string.Empty;
    }
}