using System.Collections.Generic;

namespace erp.DTOS
{
    public class CreateReturnRequestDto
    {
        public string OrderId { get; set; }
        public List<CreateReturnItemDto> Items { get; set; }
    }
}
