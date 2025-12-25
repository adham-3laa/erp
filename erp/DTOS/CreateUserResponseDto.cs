using System;
using System.Text.Json.Serialization;

namespace erp.DTOS
{
    public class CreateUserResponseDto
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("traceId")]
        public string TraceId { get; set; }

        // بيانات المستخدم الراجعة
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
        public string Usertype { get; set; }

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
