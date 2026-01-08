using System;
using System.Text.Json.Serialization;

namespace erp.DTOS.Orders
{
    public class OrderDto
    {
        // === API fields ===
        public string id { get; set; } = "";
        public int code { get; set; }
        public decimal totalamount { get; set; }
        public decimal commissionamount { get; set; }
        public string status { get; set; } = "";

        public string customername { get; set; } = "";
        public string salesrepname { get; set; } = "";

        public DateTime dateofcreation { get; set; }

        // === UI helpers (❌ ignored by JSON) ===
        [JsonIgnore]
        public string OrderId => id;

        [JsonIgnore]
        public string CustomerName => customername;

        [JsonIgnore]
        public string SalesRepName => salesrepname;

        [JsonIgnore]
        public decimal TotalAmountDisplay => totalamount;

        [JsonIgnore]
        public string CreatedAt =>
            dateofcreation.ToString("yyyy-MM-dd HH:mm");
    }
}
