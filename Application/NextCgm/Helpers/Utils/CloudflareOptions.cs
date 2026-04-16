namespace NextCgm.Helpers.Utils
{
    public class CloudflareOptions
    {
        public string ApiBaseUrl { get; set; } = "https://api.cloudflare.com/client/v4/";
        public string ApiToken { get; set; } = string.Empty;
        public string ZoneId { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string TargetIp { get; set; } = string.Empty;
    }
}
