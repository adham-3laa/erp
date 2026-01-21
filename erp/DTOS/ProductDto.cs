using System.Text.Json.Serialization;

namespace EduGate.Dtos
{
    public class ProductDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("code")]
        public int code { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        // الكمية المتاحة في المخزون
        [JsonPropertyName("availablequantity")]
        public int AvailableQuantity { get; set; }
    }
}
