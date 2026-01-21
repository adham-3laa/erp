using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace erp.DTOS
{
    public class SupplierDto
    {
        [JsonPropertyName("supplierid")]
        public string SupplierId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("contactinfo")]
        public string ContactInfo { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }
    }

    public class SuppliersListResponseDto
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("value")]
        public List<SupplierDto> Value { get; set; } = new();
    }
}
