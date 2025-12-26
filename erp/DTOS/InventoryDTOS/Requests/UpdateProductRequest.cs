namespace erp.DTOS.Inventory.Requests
{
    public class UpdateProductRequest
    {
        public string productid { get; set; } = "";
        public string productname { get; set; } = "";
        public int sellprice { get; set; }

        public int buyprice { get; set; }
        public int quantity { get; set; }
        public string sku { get; set; } = "";
        public string description { get; set; } = "";
        public string categoryid { get; set; } = "";
    }
}
