namespace NextCgm.Helpers
{
    public class DockerStatusEnums
    {
        public enum DockerContainerStatus
        {
            Created,
            Running,
            Paused,
            Restarting,
            Removing,
            Exited,
            Dead
        }
    }
}
