namespace erp.DTOS.Inventory.Requests;

public class CreateProductRequest
{
    public string productname { get; set; } = "";
    public int saleprice { get; set; }
    public int buyprice { get; set; }
    public int quantity { get; set; }
    public string sku { get; set; } = "";
    public string description { get; set; } = "";
    public string categoryid { get; set; } = "";
    public string supplierid { get; set; } = "";


}
