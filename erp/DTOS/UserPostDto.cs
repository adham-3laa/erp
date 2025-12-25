using System.Text.Json.Serialization;

namespace erp.DTOS
{
    public class UserPostDto
    {
        [JsonPropertyName("fullname")]
        public string Fullname { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        // الباك إند محتاجها في body + query
        [JsonPropertyName("usertype")]
        public string UserType { get; set; }

        [JsonPropertyName("phonenumber")]
        public string PhoneNumber { get; set; }
    }
}
