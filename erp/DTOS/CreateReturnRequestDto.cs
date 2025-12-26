using System.Collections.Generic;

namespace erp.DTOS
{
    public class CreateReturnRequestDto
    {
        public string CustomerId { get; set; }
        public string OrderId { get; set; }
        public List<CreateReturnItemDto> Items { get; set; } = new();
    }
}
