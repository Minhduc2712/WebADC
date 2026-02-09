namespace ErpOnlineOrder.Application.DTOs.OrderDTOs
{
    public class CreateOrderResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? Order_id { get; set; }
        public string? Order_code { get; set; }
        public List<ProductValidationError> Invalid_products { get; set; } = new();
    }
    public class ProductValidationError
    {
        public int Product_id { get; set; }
        public string Product_name { get; set; } = string.Empty;
        public string Error_message { get; set; } = string.Empty;
    }
}
