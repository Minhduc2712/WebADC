namespace ErpOnlineOrder.Application.DTOs.CustomerProductDTOs
{
    public class AssignProductsToCustomerDto
    {
        public int Customer_id { get; set; }
        public List<int> Product_ids { get; set; } = new List<int>();
        public decimal? Default_discount_percent { get; set; }
    }
}
