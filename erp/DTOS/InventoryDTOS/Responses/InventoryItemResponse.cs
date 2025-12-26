namespace erp.DTOS.Inventory.Responses;

public class InventoryItemResponse
{
    public string productid { get; set; } = "";
    public string productname { get; set; } = "";
    public decimal sellprice { get; set; }
    public string? categoryid { get; set; }
}
