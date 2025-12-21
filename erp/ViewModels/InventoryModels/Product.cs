namespace EduGate.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public decimal SalePrice { get; set; }
        public decimal BuyPrice { get; set; }
        public int Quantity { get; set; }
        public string Category { get; set; }
        public string Supplier { get; set; }
    }

}
