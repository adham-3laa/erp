namespace erp.DTOS.Inventory.Responses
{
    public class InventoryItemResponse
    {
        public string productid { get; set; } = "";
        public int code { get; set; }
        public string productname { get; set; } = "";

        public decimal saleprice { get; set; }
        public decimal buyprice { get; set; }

        public int quantity { get; set; }

        public string sku { get; set; } = "";
        public string description { get; set; } = "";

        public string categoryname { get; set; } = "";
        public string categoryid { get; set; } = "";
    }
}
