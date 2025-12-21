using System.Text.Json.Serialization;

namespace erp.DTOS
{
    public class UserUpdateDto
    {
        public string Fullname { get; set; }

        [JsonPropertyName("usertype")]
        public string UserType { get; set; }

        [JsonPropertyName("isactive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("imagepath")]
        public string ImagePath { get; set; }
    }
}