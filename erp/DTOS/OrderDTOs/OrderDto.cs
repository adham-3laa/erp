using System;
using System.Text.Json.Serialization;

namespace erp.DTOS.Orders
{
    public class OrderDto
    {
        // === API fields ===
        [JsonPropertyName("id")]
        public string id { get; set; } = "";
        
        [JsonPropertyName("code")]
        public int code { get; set; }
        
        [JsonPropertyName("totalamount")]
        public decimal totalamount { get; set; }
        
        [JsonPropertyName("commissionamount")]
        public decimal commissionamount { get; set; }
        
        [JsonPropertyName("status")]
        public string status { get; set; } = "";

        [JsonPropertyName("customername")]
        public string customername { get; set; } = "";
        
        [JsonPropertyName("salesrepname")]
        public string salesrepname { get; set; } = "";

        [JsonPropertyName("dateofcreation")]
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
