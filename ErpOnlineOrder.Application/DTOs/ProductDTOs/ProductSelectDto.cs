namespace ErpOnlineOrder.Application.DTOs.ProductDTOs
{
    public class ProductSelectDto
    {
        public int Id { get; set; }
        public string Product_code { get; set; } = string.Empty;
        public string Product_name { get; set; } = string.Empty;
        public string Display_text => string.IsNullOrEmpty(Product_name) ? Product_code : $"{Product_code} - {Product_name}";
    }
}
