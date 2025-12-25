using System.Text.Json.Serialization;

namespace erp.DTOS
{

    public class ChangePasswordRequest
    {
        public string NewPassword { get; set; }
    }




  

    public class ChangePasswordResponseDto
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("traceId")]
        public string TraceId { get; set; }
    }

}