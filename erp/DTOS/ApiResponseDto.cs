//namespace erp.DTOS.Inventory.Responses
//{
//    public class ApiResponse<T>
//    {
//        public int statusCode { get; set; }
//        public string message { get; set; } = "";
//        public T value { get; set; } = default!;
//    }
//}
namespace erp.DTOS
{
    public class ApiResponseForReturn<T>
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string TraceId { get; set; }
        public T Value { get; set; }
    }
}
