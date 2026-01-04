using System.Text.Json.Serialization;

public class CreateReturnItemDto
{
    [JsonPropertyName("productname")]
    public string ProductId { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("reason")]
    public string Reason { get; set; }
}
