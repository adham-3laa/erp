namespace erp.DTOS.Orders
{
    public class CreateOrderRequestDto
    {
        public string salesrepname { get; set; } = "";
        public string customername { get; set; } = "";
        public List<CreateOrderItemDto> items { get; set; } = new();
    }

    public class CreateOrderItemDto
    {
        public string productname { get; set; } = "";
        public int quantity { get; set; }
    }
}
