namespace erp.DTOS.InventoryCheck.Requests
{
    public class InventoryAdjustmentRequest
    {
        public string productid { get; set; } = "";
        public int actualquantity { get; set; }
    }
}
