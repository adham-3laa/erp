namespace erp.DTOS
{
    public class OrderItemForReturnDto
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string CustomerId { get; set; }

        // UI only
        public int ReturnQuantity { get; set; }
        public string Reason { get; set; }
    }
}
