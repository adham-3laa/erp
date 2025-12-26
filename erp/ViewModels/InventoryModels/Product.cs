namespace EduGate.Models
{
    public class Product
    {
        // ===== ID =====
        public string ProductId { get; set; } = "";

        // ===== Basic Info =====
        public string Name { get; set; } = "";
        public int SalePrice { get; set; }
        public int BuyPrice { get; set; }
        public int Quantity { get; set; }

        // ===== Extra =====
        public string SKU { get; set; } = "";
        public string Description { get; set; } = "";
        public string Category { get; set; } = "";

        // ⚠️ يستخدم فقط في GetAll / UI
        public string Supplier { get; set; } = "";
    }
}
