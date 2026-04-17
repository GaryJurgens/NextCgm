namespace NextCgm.Helpers.Utils
{
    public class DockerOptions
    {
        public string ImageName { get; set; } = string.Empty;
        public string Tag { get; set; } = "latest";
        public string ContainerName { get; set; } = string.Empty;
        public int ContainerPort { get; set; } = 80;

        public string EndDomain { get; set; } = string.Empty;

        public List<string> EnvironmentVariables { get; set; } = new List<string>();

        
    }

    public class NginxUnitOptions
    {
        public string ApiBaseUrl { get; set; }
        public string DefaultAppRoot { get; set; }

        public string CertificatePath { get; set; }

        public string KeyPath { get; set; }

        public string ConfigDirectory { get; set; } = "/app/nginx_conf";
        public string NginxContainerName { get; set; } = "nginx-ui";

        // This comes from AppConfig
    }
}