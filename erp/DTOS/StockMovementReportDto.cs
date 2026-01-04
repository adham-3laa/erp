using System.Text.Json.Serialization;

public class StockMovementReportDto
{
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("traceId")]
    public string TraceId { get; set; }

    [JsonPropertyName("productid")]
    public string ProductId { get; set; }

    [JsonPropertyName("productname")]
    public string ProductName { get; set; }

    [JsonPropertyName("totalin")]
    public int TotalIn { get; set; }

    [JsonPropertyName("totalinpurchased")]
    public int TotalInPurchased { get; set; }

    [JsonPropertyName("totalinreturned")]
    public int TotalInReturned { get; set; }

    [JsonPropertyName("totalinadjusted")]
    public int TotalInAdjusted { get; set; }

    [JsonPropertyName("totalout")]
    public int TotalOut { get; set; }

    [JsonPropertyName("totaloutsold")]
    public int TotalOutSold { get; set; }

    [JsonPropertyName("totaloutadjusted")]
    public int TotalOutAdjusted { get; set; }

    [JsonPropertyName("currentstock")]
    public int CurrentStock { get; set; }
}
