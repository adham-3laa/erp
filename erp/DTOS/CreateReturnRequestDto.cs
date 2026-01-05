using System.Text.Json.Serialization;

public class CreateReturnRequestDto
{
    [JsonPropertyName("orderid")]
    public string OrderId { get; set; }

    [JsonPropertyName("items")]
    public List<CreateReturnItemDto> Items { get; set; }
}
