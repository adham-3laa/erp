namespace EduGate.Models
{
    public class Product
    {
        public string ProductId { get; set; } = "";
        public string Name { get; set; } = "";

        public int SalePrice { get; set; }
        public int BuyPrice { get; set; }
        public int Quantity { get; set; }

        public string Category { get; set; } = "";
        public string Supplier { get; set; } = "";
        public string SKU { get; set; } = "";
        public string Description { get; set; } = "";
    }
}
