using System.Text.Json.Serialization;

namespace erp.DTOS
{
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

        [JsonPropertyName("totalout")]
        public int TotalOut { get; set; }

        [JsonPropertyName("currentstock")]
        public int CurrentStock { get; set; }
    }
}
