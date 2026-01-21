using System.Text.Json.Serialization;

namespace erp.DTOS
{
    public class CreateReturnItemDto
    {
        [JsonPropertyName("productname")]
        public string ProductName { get; set; } = "";

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = "";
    }
}
