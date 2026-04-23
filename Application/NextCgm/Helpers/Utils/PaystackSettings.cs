namespace NextCgm.Helpers.Utils
{
    public class PaystackSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
        public string CallbackUrl { get; set; } = string.Empty;
    }
}