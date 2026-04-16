namespace NextCgm.Helpers.Utils
{
    public class DockerOptions
    {
        public string ImageName { get; set; } = string.Empty;
        public string Tag { get; set; } = "latest";
        public string ContainerName { get; set; } = string.Empty;
        public int ContainerPort { get; set; } = 80;

        public string EndDomain { get; set; } = string.Empty;

        
    }

    public class NginxUnitOptions
    {
        public string ApiBaseUrl { get; set; }
        public string DefaultAppRoot { get; set; }

        public string SSLCertPath { get; set; }

        public string SSLKeyPath { get; set; }

        // This comes from AppConfig
    }
}