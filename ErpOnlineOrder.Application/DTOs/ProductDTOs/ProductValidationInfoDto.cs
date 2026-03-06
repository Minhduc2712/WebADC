namespace ErpOnlineOrder.Application.DTOs.ProductDTOs
{
    public class ProductValidationInfoDto
    {
        public int Id { get; set; }
        public string Product_name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int? Max_quantity { get; set; }
        public bool HasSetting { get; set; }
    }
}