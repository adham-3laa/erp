using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace erp.DTOS.Dashboard
{
    public sealed class DashboardStatsResponse
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("traceId")]
        public string? TraceId { get; set; }

        [JsonPropertyName("totalsalestoday")]
        public decimal TotalSalesToday { get; set; }

        [JsonPropertyName("totalprofittoday")]
        public decimal TotalProfitToday { get; set; }

        [JsonPropertyName("pendingorderscount")]
        public int PendingOrdersCount { get; set; }

        [JsonPropertyName("approvedorderscounttoday")]
        public int ApprovedOrdersCountToday { get; set; }

        [JsonPropertyName("lowstockproducts")]
        public List<LowStockProductDto> LowStockProducts { get; set; } = new();
    }

    public sealed class LowStockProductDto
    {
        [JsonPropertyName("productid")]
        public string ProductId { get; set; } = "";

        [JsonPropertyName("productname")]
        public string ProductName { get; set; } = "";

        [JsonPropertyName("currentquantity")]
        public int CurrentQuantity { get; set; }

        [JsonPropertyName("sku")]
        public string Sku { get; set; } = "";
    }
}
