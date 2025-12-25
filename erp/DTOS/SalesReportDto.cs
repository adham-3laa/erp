using System;
using System.Text.Json.Serialization;

namespace erp.DTOS
{
    public class SalesReportDto
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("traceId")]
        public string TraceId { get; set; }

        [JsonPropertyName("fromdate")]
        public DateTime FromDate { get; set; }

        [JsonPropertyName("todate")]
        public DateTime ToDate { get; set; }

        [JsonPropertyName("totalsales")]
        public decimal TotalSales { get; set; }

        [JsonPropertyName("totalorders")]
        public int TotalOrders { get; set; }
    }
}
