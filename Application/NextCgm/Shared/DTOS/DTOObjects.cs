using NextCgm.Shared.ApiViewModels;

namespace NextCgm.Shared.DTOS
{
    public class DTOObjects
    {
    }

    public class LoginRequestDTO
    {
        public string EmailUsername { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; }
        public UserApiViewModel? Payload { get; set; }

        public static LoginResponseDTO Failure(string msg)
        {
            return new LoginResponseDTO
            {
                Success = false,
                Message = msg,
                Token = string.Empty,
                Payload = null
            };
        }
    }

    public class RemoveUserRequestDTO
    {
        public Guid UserId { get; set; }
    }

    public class RemoveUserResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public static RemoveUserResponseDTO Failure(string msg)
        {
            return new RemoveUserResponseDTO
            {
                Success = false,
                Message = msg
            };
        }
    }

    public class LogoutResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public static LogoutResponseDTO Failure(string msg)
        {
            return new LogoutResponseDTO
            {
                Success = false,
                Message = msg
            };
        }
    }

    public class ForgotPasswordRequestDTO
    {
        public string EmailUsername { get; set; }
    }

    public class ForgotPasswordResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public static ForgotPasswordResponseDTO Failure(string msg)
        {
            return new ForgotPasswordResponseDTO
            {
                Success = false,
                Message = msg
            };
        }
    }

    public class ResetPasswordRequestDTO
    {
        public string EmailUsername { get; set; }
        public string VerificationCode { get; set; }
        public string NewPassword { get; set; }
    }

    public class ResetPasswordResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public static ResetPasswordResponseDTO Failure(string msg)
        {
            return new ResetPasswordResponseDTO
            {
                Success = false,
                Message = msg
            };
        }
    }

    public class VerifyOtpRequestDTO
    {
        public string EmailUsername { get; set; }
        public string VerificationCode { get; set; }
    }

    public class VerifyOtpResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } // Optional, if verifying OTP logs them in
        public UserApiViewModel? Payload { get; set; }

        public static VerifyOtpResponseDTO Failure(string msg)
        {
            return new VerifyOtpResponseDTO
            {
                Success = false,
                Message = msg,
                Token = string.Empty,
                Payload = null
            };
        }
    }

    public class CreateUserResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
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
        public string Message { get; set; } = string.Empty;
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
        public string Message { get; set; } = string.Empty;
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
        public string Message { get; set; } = string.Empty;
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
        public string Message { get; set; } = string.Empty;
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
        public string Message { get; set; } = string.Empty;
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
        public string Message { get; set; } = string.Empty;
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
        public string Message { get; set; } = string.Empty;
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
        public string Message { get; set; } = string.Empty;

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
        public string Message { get; set; } = string.Empty;
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
        public string Message { get; set; } = string.Empty;
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
        public string Message { get; set; } = string.Empty;
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
        public string Message { get; set; } = string.Empty;
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

    public class StopContainerRequestDTO
    {
        public Guid DockerContainersID { get; set; }
    }

    public class StopContainerResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ContainerApiViewModel? Payload { get; set; }

        public static StopContainerResponseDTO Failure(string msg)
        {
            return new StopContainerResponseDTO
            {
                Success = false,
                Message = msg,
                Payload = null
            };
        }
    }

    public class CreateDnsRecordRequestDTO
    {
        public string Subdomain { get; set; } = string.Empty;
        public string RecordType { get; set; } = "A"; // "A" or "CNAME"
        public string? Target { get; set; } // If null, uses TargetIp from config for A records
        public bool Proxied { get; set; } = true;
    }

    public class CreateDnsRecordResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string RecordId { get; set; } = string.Empty;

        public static CreateDnsRecordResponseDTO Failure(string msg)
        {
            return new CreateDnsRecordResponseDTO
            {
                Success = false,
                Message = msg
            };
        }
    }

    public class RemoveDnsRecordRequestDTO
    {
        public string RecordId { get; set; } = string.Empty;
    }

    public class RemoveDnsRecordResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public static RemoveDnsRecordResponseDTO Failure(string msg)
        {
            return new RemoveDnsRecordResponseDTO
            {
                Success = false,
                Message = msg
            };
        }
    }

    public class CountryDTO
    {
        public Guid CountryListID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Iso2 { get; set; } = string.Empty;
    }

    public class ProvinceStateDTO
    {
        public Guid ProvinceStateListID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
    }

    public class TimeZoneDTO
    {
        public Guid TimeZoneDataID { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public string GmtOffsetName { get; set; } = string.Empty;
    }

    public class GetCountriesResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<CountryDTO>? Payload { get; set; }
    }

    public class GetStatesResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<ProvinceStateDTO>? Payload { get; set; }
    }

    public class GetTimeZonesResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<TimeZoneDTO>? Payload { get; set; }
    }
}