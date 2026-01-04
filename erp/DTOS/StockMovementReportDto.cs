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

    // ✅ NEW
    [JsonPropertyName("totalinupdatedbyemployee")]
    public int TotalInUpdatedByEmployee { get; set; }

    [JsonPropertyName("totalout")]
    public int TotalOut { get; set; }

    [JsonPropertyName("totaloutsold")]
    public int TotalOutSold { get; set; }

    [JsonPropertyName("totaloutadjusted")]
    public int TotalOutAdjusted { get; set; }

    // ✅ NEW
    [JsonPropertyName("totaloutupdatedbyemployee")]
    public int TotalOutUpdatedByEmployee { get; set; }

    [JsonPropertyName("currentstock")]
    public int CurrentStock { get; set; }
}
