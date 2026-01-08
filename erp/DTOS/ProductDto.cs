namespace EduGate.Dtos
{
    public class ProductDto
    {
        public int Id { get; set; }
        public int code { get; set; }

        public string Name { get; set; }

        // الكمية المتاحة في المخزون
        public int AvailableQuantity { get; set; }
    }
}
