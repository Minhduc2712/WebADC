namespace ErpOnlineOrder.Application.DTOs.CustomerCategoryDTOs
{
    public class CustomerCategoryDto
    {
        public int Id { get; set; }
        public int Customer_id { get; set; }
        public string Customer_name { get; set; } = string.Empty;
        public int Category_id { get; set; }
        public string Category_code { get; set; } = string.Empty;
        public string Category_name { get; set; } = string.Empty;
        public decimal? Discount_percent { get; set; }
        public bool Is_active { get; set; }
        public DateTime Created_at { get; set; }
    }
}
