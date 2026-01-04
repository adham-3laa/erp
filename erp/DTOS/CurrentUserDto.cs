using System;
using System.Text.Json.Serialization;

namespace erp.DTOS
{
    public class CurrentUserApiResponse<T>
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("traceId")]
        public string TraceId { get; set; }

        // لو عندك APIs بتبعت data
        [JsonPropertyName("data")]
        public T Data { get; set; }
    }

    // ✅ ده مطابق 100% لريسبونس /api/users/get-user
    public class CurrentUserDto
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("traceId")]
        public string TraceId { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("fullname")]
        public string Fullname { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("phonenumber")]
        public string Phonenumber { get; set; }

        [JsonPropertyName("usertype")]
        public string UserType { get; set; }

        [JsonPropertyName("isactive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("imagepath")]
        public string ImagePath { get; set; }

        [JsonPropertyName("dateofcreation")]
        public DateTime DateOfCreation { get; set; }

        [JsonPropertyName("farmscount")]
        public int FarmsCount { get; set; }
    }
}
