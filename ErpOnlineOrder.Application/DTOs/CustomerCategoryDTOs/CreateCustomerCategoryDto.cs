namespace ErpOnlineOrder.Application.DTOs.CustomerCategoryDTOs
{
    public class CreateCustomerCategoryDto
    {
        public int Customer_id { get; set; }
        public int Category_id { get; set; }
        public decimal? Discount_percent { get; set; }
        public bool Is_active { get; set; } = true;
    }
}
