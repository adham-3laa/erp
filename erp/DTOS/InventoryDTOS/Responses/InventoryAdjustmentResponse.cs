namespace erp.DTOS.InventoryCheck.Responses
{
    public class InventoryAdjustmentResponse
    {
        public int statusCode { get; set; }
        public string message { get; set; } = "";

        // موجودة فقط لو UpdateStock = true
        public int oldquantity { get; set; }
        public int newquantity { get; set; }

        // موجودة لو UpdateStock = false
        public int systemquantity { get; set; }

        public decimal valuedifference { get; set; }
        public string financialimpact { get; set; } = "";
    }

}
