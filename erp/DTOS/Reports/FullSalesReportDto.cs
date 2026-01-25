using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace erp.DTOS.Reports
{
    public class FullSalesReportDto
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

        [JsonPropertyName("totalorders")]
        public int TotalOrders { get; set; }

        [JsonPropertyName("totalrevenue")]
        public decimal TotalRevenue { get; set; }

        [JsonPropertyName("totalcost")]
        public decimal TotalCost { get; set; }

        [JsonPropertyName("netprofit")]
        public decimal NetProfit { get; set; }

        [JsonPropertyName("profitmargin")]
        public decimal ProfitMargin { get; set; }

        [JsonPropertyName("dailytrend")]
        public List<DailyTrendDto> DailyTrend { get; set; } = new();

        [JsonPropertyName("topsellingproducts")]
        public List<TopSellingProductDto> TopSellingProducts { get; set; } = new();

        [JsonPropertyName("topcustomers")]
        public List<TopCustomerDto> TopCustomers { get; set; } = new();
    }

    public class DailyTrendDto
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("revenue")]
        public decimal Revenue { get; set; }

        [JsonPropertyName("profit")]
        public decimal Profit { get; set; }

        [JsonPropertyName("ordercount")]
        public int OrderCount { get; set; }
    }

    public class TopSellingProductDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("totalvalue")]
        public decimal TotalValue { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    public class TopCustomerDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("totalvalue")]
        public decimal TotalValue { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}
