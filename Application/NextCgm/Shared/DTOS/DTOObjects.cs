using NextCgm.Shared.ApiViewModels;

namespace NextCgm.Shared.DTOS
{
    public class DTOObjects
    {
        

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

    public class GetUserRequestDTO
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


}
