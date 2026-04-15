namespace NextCgm.DataEntities.DocumentDatabases
{
    public class UserContainerDatabase
    {
        public Guid UserContainerDatabaseID { get; set; } = Medo.Uuid7.NewUuid7();

        public Guid UserEntityID { get; set; }

        public string ClusterName { get; set; } = string.Empty;

        public string DatabaseName { get; set; } = string.Empty;

        public string ConnectionString { get; set; } = string.Empty; // mongo style not Serv +
    }
}