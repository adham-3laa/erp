namespace erp.DTOS.Inventory.Responses
{
    public class ApiResponse<T>
    {
        public int statusCode { get; set; }
        public string message { get; set; } = "";
        public T value { get; set; } = default!;
    }
}
