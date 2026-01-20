using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace erp.DTOS.Cheques
{
    /// <summary>
    /// Handles inconsistent API response where status can be either a string ("Pending") or an integer (0).
    /// </summary>
    public class StatusJsonConverter : JsonConverter<string>
    {
        // Map integer status values to their string representations
        private static readonly string[] StatusMap = { "Pending", "Collected", "Rejected", "Cancelled" };

        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return reader.GetString() ?? "Pending";
                    
                case JsonTokenType.Number:
                    int statusInt = reader.GetInt32();
                    return (statusInt >= 0 && statusInt < StatusMap.Length) 
                        ? StatusMap[statusInt] 
                        : "Pending";
                        
                case JsonTokenType.Null:
                    return "Pending";
                    
                default:
                    throw new JsonException($"Unexpected token type for status: {reader.TokenType}");
            }
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }

    public class ChequeDto
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

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

        [JsonPropertyName("status")]
        [JsonConverter(typeof(StatusJsonConverter))]
        public string Status { get; set; } = "";

        [JsonPropertyName("relatedname")]
        public string RelatedName { get; set; } = "";

        [JsonPropertyName("notes")]
        public string Notes { get; set; } = "";

        [JsonPropertyName("issuedate")]
        public DateTime IssueDate { get; set; }
    }
}
