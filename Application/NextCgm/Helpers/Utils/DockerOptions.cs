namespace NextCgm.Helpers.Utils
{
    public class DockerOptions
    {
        public string ImageName { get; set; } = string.Empty;
        public string Tag { get; set; } = "latest";
        public string ContainerName { get; set; } = string.Empty;
        public int ContainerPort { get; set; } = 80;

        public string EndDomain { get; set; } = string.Empty;
        public bool IsLocalDevelopment { get; set; } = true;
        public string DockerNetworkName { get; set; } = "nextcgm_network";

        public List<string> EnvironmentVariables { get; set; } = new List<string>();
        public string TargetIp { get; set; } = String.Empty;
    }

    public class NginxProxyManagerOptions
    {
        public string ApiBaseUrl { get; set; } = "http://nginx-proxy-manager:81/api";
        public string Email { get; set; } = "admin@example.com";
        public string Password { get; set; } = "changeme";
    }
}