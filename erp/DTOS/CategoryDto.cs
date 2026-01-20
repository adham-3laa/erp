using System.Text.Json.Serialization;

namespace erp.DTOs;

public sealed class CategoryDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";
    
    [JsonPropertyName("code")]
    public int code { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("dateofcreation")]
    public DateTime? DateOfCreation { get; set; }   // ✅ جديد
}
