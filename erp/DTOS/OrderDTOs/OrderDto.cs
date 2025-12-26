using System;

namespace erp.DTOS.Orders
{
    public class OrderDto
    {
        // === API fields ===
        public string id { get; set; } = "";
        public decimal totalamount { get; set; }
        public decimal commissionamount { get; set; }
        public string status { get; set; } = "";
        public string customerid { get; set; } = "";   // ✅ string
        public string salesrepid { get; set; } = "";   // ✅ string
        public DateTime dateofcreation { get; set; }

        // === UI helpers ===
        public string OrderId => id;
        public decimal TotalAmountDisplay => totalamount;
        public string CreatedAt =>
            dateofcreation.ToString("yyyy-MM-dd HH:mm");
    }
}
