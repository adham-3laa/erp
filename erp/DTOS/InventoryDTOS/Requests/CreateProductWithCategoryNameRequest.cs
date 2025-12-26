namespace erp.DTOS.Inventory.Requests
{
    public class CreateProductWithCategoryNameRequest
    {
        public string productname { get; set; } = "";
        public int saleprice { get; set; }
        public int buyprice { get; set; }
        public int quantity { get; set; }
        public string sku { get; set; } = "";
        public string description { get; set; } = "";
        public string categoryname { get; set; } = "";

        // ⭐ REQUIRED BY API
        public string supplierid { get; set; } = "";
    }
}
