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
