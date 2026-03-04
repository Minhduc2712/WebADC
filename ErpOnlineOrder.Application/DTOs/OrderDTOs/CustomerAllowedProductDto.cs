namespace ErpOnlineOrder.Application.DTOs.OrderDTOs
{
    public class CustomerAllowedProductDto
    {
        public int Product_id { get; set; }
        public string Product_code { get; set; } = string.Empty;
        public string Product_name { get; set; } = string.Empty;
        public decimal Original_price { get; set; }
        public decimal? Custom_price { get; set; }
        public decimal? Discount_percent { get; set; }
        public decimal Final_price { get; set; }
        public int? Max_quantity { get; set; }
        public string Category_name { get; set; } = string.Empty;
    }
}
