namespace ErpOnlineOrder.Application.DTOs.CustomerProductDTOs
{
    public class CustomerProductDto
    {
        public int Id { get; set; }
        public int Customer_id { get; set; }
        public string Customer_name { get; set; } = string.Empty;
        public int Product_id { get; set; }
        public string Product_code { get; set; } = string.Empty;
        public string Product_name { get; set; } = string.Empty;
        public string Original_price { get; set; } = string.Empty;
        public decimal? Custom_price { get; set; }
        public decimal? Discount_percent { get; set; }
        public int? Max_quantity { get; set; }
        public bool Is_active { get; set; }
        public DateTime Created_at { get; set; }
    }
}
