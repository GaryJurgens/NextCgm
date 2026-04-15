using Medo;

namespace NextCgm.DataEntities.Nginx
{
    public class NginxSyncLog
    {
        public Guid NginxSyncLogID = Uuid7.NewGuid();
        public long Id { get; set; }
        public Guid ContainerEntityId { get; set; }
        public DateTime SyncTimestamp { get; set; }

        public bool WasSuccessful { get; set; }
        public string LastErrorCode { get; set; }
        public string RawJsonSent { get; set; } // Good for debugging API failures
    }
}
