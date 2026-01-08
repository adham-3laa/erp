namespace erp.DTOs;

public sealed class CategoryDto
{
    public string Id { get; set; } = "";
    public int code { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }

    public DateTime? DateOfCreation { get; set; }   // ✅ جديد
}
