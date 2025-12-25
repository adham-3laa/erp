namespace erp.DTOs;

public sealed class CreateCategoryApiResponse
{
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public string? TraceId { get; set; }
    public string? CategoryId { get; set; } // maps from "categoryid"
}
