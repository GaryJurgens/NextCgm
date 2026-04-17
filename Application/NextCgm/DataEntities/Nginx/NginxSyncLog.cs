using Medo;

namespace NextCgm.DataEntities.Nginx
{
    public class NginxSyncLog
    {
        public Guid NginxSyncLogID { get; set; } = Uuid7.NewUuid7();
        public long Id { get; set; }
        public Guid ContainerEntityId { get; set; }
        public DateTime SyncTimestamp { get; set; }

        public bool WasSuccessful { get; set; }
        public string LastErrorCode { get; set; } = string.Empty;
        public string RawJsonSent { get; set; } = string.Empty; // Good for debugging API failures
    }
}
