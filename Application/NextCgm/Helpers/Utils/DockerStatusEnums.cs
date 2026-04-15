namespace NextCgm.Helpers.Utils
{
    public enum DockerContainerStatus
    {
        Created,
        Running,
        Paused,
        Restarting,
        Removing,
        Exited,
        Dead,
        Pending,
        Pulled,
        TimeOutError,
        Error,
        Creating,
    }
}