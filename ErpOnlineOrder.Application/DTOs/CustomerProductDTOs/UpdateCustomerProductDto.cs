namespace ErpOnlineOrder.Application.DTOs.CustomerProductDTOs
{

    public class UpdateCustomerProductDto
    {
        public int Id { get; set; }
        public decimal? Custom_price { get; set; }
        public decimal? Discount_percent { get; set; }
        public int? Max_quantity { get; set; }
        public bool Is_active { get; set; }
    }
}
