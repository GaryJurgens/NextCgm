using NextCgm.DataEntities.User;

namespace NextCgm.Shared.ApiViewModels
{
    public class ContainerApiViewModel
    {
        public Guid UserEntityID { get; set; }
        public UserEntity? UserEntity { get; set; }

        public string BaseImageContaierName { get; set; } = string.Empty;

        public string FriendlyContainerURL { get; set; } = string.Empty; // this is the URL that the user will use to access the container, and is used for mapping to the host machine. This can be a combination of the base image name and a unique identifier, such as Nightscout-Mary-1234567890, where Nightscout-Mary is the base image name, and 1234567890 is a unique identifier for this instance. This is used to create a unique container name and to identify the container in the system.

        public string InstanceUserNameIdentifer { get; set; } = string.Empty; // this some thing Nightscout-Mary-1234567890, where Nightscout-Mary is the base image name, and 1234567890 is a unique identifier for this instance. This is used to create a unique container name and to identify the container in the system.

        public string InstanceID { get; set; } = string.Empty; // this is a unique identifier for this instance, and is used to create a unique container name and to identify the container in the system. This can be a combination of the base image name and a unique identifier, such as Nightscout-Mary-1234567890, where Nightscout-Mary is the base image name, and 1234567890 is a unique identifier for this instance.

        public int PortLeft { get; set; } = 0; // Left port must be unique across all containers, and is used for mapping to the host machine. The right port is the internal port used by the application inside the container, and can be the same for all containers if they use the same base image and application configuration.
        public int PortRight { get; set; } = 0; // Right port is the internal port used by the application inside the container, and can be the same for all containers if they use the same base image and application configuration.

        public string DockerStatus { get; set; } = string.Empty;

        public string DataBaseConnectionString { get; set; } = string.Empty;

        // environment variables for the container, stored as a JSON string. This can include any environment variables that are needed for the application inside the container, such as database connection strings, API keys, etc. The JSON string can be deserialized into a dictionary or a custom class when needed.
        public string EnvironmentVariables { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime StoppedAt { get; set; }
        public DateTime RemovedAt { get; set; }
        public string ImageNameInUse { get; set; }
        public int ExposedPortLeft { get; set; }
        public int HostPortRight { get;  set; }
    }
}