using System.Collections.Generic;

namespace erp.DTOS.Orders
{
    public class CreateOrderRequestDto
    {
        public string salesrepid { get; set; } = "";
        public string customerid { get; set; } = "";
        public List<CreateOrderItemDto> items { get; set; } = new();
    }

    public class CreateOrderItemDto
    {
        public string productid { get; set; } = "";
        public int quantity { get; set; }
    }
}
