using System.Text.Json.Serialization;

namespace erp.DTOS
{
    public class CommissionReportResponseDto
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("traceId")]
        public string TraceId { get; set; }

        [JsonPropertyName("value")]
        public List<CommissionReportItemDto> Value { get; set; }
    }

  public class CommissionReportItemDto
    {
        public string SalesRepId { get; set; }
        public string SalesRepName { get; set; }
        public decimal TotalCommission { get; set; }
        public int TotalOrdersConfirmed { get; set; }
    }

}
