using NextCgm.Shared.ApiViewModels;

namespace NextCgm.Shared.DTOS
{
    public class DTOObjects
    {
    }


    public class CreateUserResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public UserApiViewModel? Payload { get; set; }
        // Static helper for quick errors
        public static CreateUserResponseDTO Failure(string msg)
        {
            return new CreateUserResponseDTO
            {
                Success = false,
                Message = msg,
                Payload = null
            };
        }
    }

    public class  CreateUserRequestDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public UserApiViewModel? Payload { get; set; }
        // Static helper for quick errors
        public static CreateUserResponseDTO Failure(string msg)
        {
            return new CreateUserResponseDTO
            {
                Success = false,
                Message = msg,
                Payload = null
            };
        }

    }

    public class GetUserResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public UserApiViewModel? Payload { get; set; }

        // Static helper for quick errors
        public static GetUserResponseDTO Failure(string msg)
        {
            return new GetUserResponseDTO
            {
                Success = false,
                Message = msg,
                Payload = null
            };
        }
    }

  

    public class CreateContainerResponseDTO // this is the response DTO for the GetContainer API endpoint, which is used to get the container information for a user. This
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public ContainerApiViewModel? Payload { get; set; }

        // Static helper for quick errors
        public static CreateContainerResponseDTO Failure(string msg)
        {
            return new CreateContainerResponseDTO
            {
                Success = false,
                Message = msg,
                Payload = null
            };
        }
    }

    public class GetContainerRequestDTO // single container request, for example when creating a new container, or getting the status of a specific container. For getting all containers for a user, we can use a different DTO that returns a list of containers.
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public ContainerApiViewModel? Payload { get; set; }

        // Static helper for quick errors
        public static GetContainerRequestDTO Failure(string msg)
        {
            return new GetContainerRequestDTO
            {
                Success = false,
                Message = msg,
                Payload = null
            };
        }
    }

    public class GetAllContainersResponseDTO // this is the response DTO for the GetAllContainers API endpoint, which is used to get all the containers for a user. This can return a list of containers, and can also include pagination information if needed.
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<ContainerApiViewModel>? Payload { get; set; }

        // Static helper for quick errors
        public static GetAllContainersResponseDTO Failure(string msg)
        {
            return new GetAllContainersResponseDTO
            {
                Success = false,
                Message = msg,
                Payload = null
            };
        }
    }

    public class GetAllContainersRequestDTO // this is the request DTO for the GetAllContainers API endpoint, which is used to get all the containers for a user. This can include filters for pagination, such as page number and page size, or filters for container status, etc.
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<ContainerApiViewModel>? Payload { get; set; }

        // Static helper for quick errors
        public static GetAllContainersRequestDTO Failure(string msg)
        {
            return new GetAllContainersRequestDTO
            {
                Success = false,
                Message = msg,
                Payload = null
            };
        }
    }

    public class CreateContainerRequestDTO // this is the request DTO for the CreateContainer API endpoint, which is used to create a new container for a user. This can include the necessary information for creating a container, such as the base image name, environment variables, etc.
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public ContainerApiViewModel? Payload { get; set; }

        // Static helper for quick errors
        public static CreateContainerRequestDTO Failure(string msg)
        {
            return new CreateContainerRequestDTO
            {
                Success = false,
                Message = msg,
                Payload = null
            };
        }
    }

    public class GetAllContinaersByUserRequestDTO // this is the request DTO for the GetAllContainersByUser API endpoint, which is used to get all the containers for a specific user. This can include the user ID as a parameter, and can also include filters for pagination, container status, etc.
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public int CurrentPage { get; set; } = 1; // for pagination, default to page 1
        public int PageSize { get; set; } = 10; // for pagination, default to 10 items per page
        public List<ContainerApiViewModel>? Payload { get; set; }

        // Static helper for quick errors
        public static GetAllContinaersByUserRequestDTO Failure(string msg)
        {
            return new GetAllContinaersByUserRequestDTO
            {
                Success = false,
                Message = msg,
                Payload = null
            };
        }
    }

    public class CreateNginxMappingRequestDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public NginxApiViewModel? Payload { get; set; }

        public static CreateNginxMappingRequestDTO Failure(string msg)
        {
            return new CreateNginxMappingRequestDTO
            {
                Success = false,
                Message = msg,
                Payload = null
            };
        }
    }

    public class CreateNginxMappingResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public NginxApiViewModel? Payload { get; set; }

        public static CreateNginxMappingResponseDTO Failure(string msg)
        {
            return new CreateNginxMappingResponseDTO
            {
                Success = false,
                Message = msg,
                Payload = null
            };
        }
    }

    public class CreateDocumentDbRequestDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public DocumentDbApiViewModel? Payload { get; set; }

        public static CreateDocumentDbRequestDTO Failure(string msg)
        {
            return new CreateDocumentDbRequestDTO
            {
                Success = false,
                Message = msg,
                Payload = null
            };
        }
    }

    public class CreateDocumentDbResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public DocumentDbApiViewModel? Payload { get; set; }

        public static CreateDocumentDbResponseDTO Failure(string msg)
        {
            return new CreateDocumentDbResponseDTO
            {
                Success = false,
                Message = msg,
                Payload = null
            };
        }
    }
}