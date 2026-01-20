using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace erp.DTOS
{
    public class ExpenseResponseDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        [JsonPropertyName("code")]
        public int code { get; set; }
        
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
        
        [JsonPropertyName("description")]
        public string Description { get; set; }
        
        [JsonPropertyName("createdat")]
        public DateTime CreatedAt { get; set; }
        
        [JsonPropertyName("accountantuserid")]
        public string AccountantUserId { get; set; }
        
        [JsonPropertyName("accountantname")]
        public string AccountantName { get; set; }
    }
}
