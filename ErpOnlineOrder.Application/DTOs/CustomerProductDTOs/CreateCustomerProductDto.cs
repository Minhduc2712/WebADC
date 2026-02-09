namespace ErpOnlineOrder.Application.DTOs.CustomerProductDTOs
{
    public class CreateCustomerProductDto
    {
        public int Customer_id { get; set; }
        public int Product_id { get; set; }
        public decimal? Custom_price { get; set; }
        public decimal? Discount_percent { get; set; }
        public int? Max_quantity { get; set; }
        public bool Is_active { get; set; } = true;
    }
}
