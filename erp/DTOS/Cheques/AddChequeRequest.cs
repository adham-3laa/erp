using System;
using System.Text.Json.Serialization;

namespace erp.DTOS.Cheques
{
    public class AddChequeRequest
    {
        [JsonPropertyName("checknumber")]
        public string CheckNumber { get; set; } = "";

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("duedate")]
        public DateTime DueDate { get; set; }

        [JsonPropertyName("bankname")]
        public string BankName { get; set; } = "";

        [JsonPropertyName("isincoming")]
        public bool IsIncoming { get; set; }

        [JsonPropertyName("relatedname")]
        public string RelatedName { get; set; } = "";

        [JsonPropertyName("notes")]
        public string Notes { get; set; } = "";
    }
}
