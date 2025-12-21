namespace erp.DTOs;

public sealed class UpdateCategoryRequest
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
}
