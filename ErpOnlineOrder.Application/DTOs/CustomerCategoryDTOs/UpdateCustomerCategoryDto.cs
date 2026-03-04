namespace ErpOnlineOrder.Application.DTOs.CustomerCategoryDTOs
{
    public class UpdateCustomerCategoryDto
    {
        public int Id { get; set; }
        public decimal? Discount_percent { get; set; }
        public bool Is_active { get; set; }
    }
}
