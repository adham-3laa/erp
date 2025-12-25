using System.Text.Json.Serialization;

namespace erp.DTOS
{
    public class ChangePasswordRequestDto
    {
        [JsonPropertyName("newPassword")]  // تأكد من الاسم الصحيح
        public string NewPassword { get; set; }
    }

    public class ChangePasswordResponseDto
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string TraceId { get; set; }
    }


    // يمكنك حذف ChangePasswordRequest القديم
}