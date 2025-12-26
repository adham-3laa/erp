using System.Text.Json.Serialization;

public class OrderItemForReturnDto
{
    [JsonPropertyName("productid")]
    public string ProductId { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("unitprice")]
    public decimal UnitPrice { get; set; }

    [JsonPropertyName("customerid")]
    public string CustomerId { get; set; }

    public int ReturnQuantity { get; set; }
    public string Reason { get; set; }
}
