using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace erp.DTOS
{
    public class ReturnToSupplierRequestDto
    {
        [JsonPropertyName("suppliername")]
        public string SupplierName { get; set; } = "";

        [JsonPropertyName("items")]
        public List<CreateReturnItemDto> Items { get; set; } = new();
    }
}
