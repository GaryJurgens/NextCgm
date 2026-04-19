using Medo;

namespace NextCgm.DataEntities.Cloudflare
{
    public class CloudflareLogger
    {
        public Guid CloudflareLoggerID { get; set; } = Uuid7.NewUuid7();
        public string Action { get; set; } = string.Empty; // Create, Delete, Update
        public string Subdomain { get; set; } = string.Empty;
        public string RecordType { get; set; } = string.Empty;
        
        // Store JSON payloads for error hunting
        public string RequestPayload { get; set; } = string.Empty;
        public string ResponsePayload { get; set; } = string.Empty;
        
        public bool IsSuccess { get; set; }
        public string ExceptionMessage { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}