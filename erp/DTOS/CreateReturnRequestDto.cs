using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace erp.DTOS
{
    public class CreateReturnRequestDto
    {
        [JsonPropertyName("ordercode")]
        public int OrderCode { get; set; }

        [JsonPropertyName("items")]
        public List<CreateReturnItemDto> Items { get; set; } = new();
    }
}
