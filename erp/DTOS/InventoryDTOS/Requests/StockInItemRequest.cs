namespace erp.DTOS.Inventory.Requests
{
    public class StockInItemRequest
    {
        public string productname { get; set; } = "";
        public decimal buyprice { get; set; }
        public int quantity { get; set; }
    }
}
