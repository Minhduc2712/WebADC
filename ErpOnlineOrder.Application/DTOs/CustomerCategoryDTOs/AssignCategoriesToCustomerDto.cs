namespace ErpOnlineOrder.Application.DTOs.CustomerCategoryDTOs
{
    public class AssignCategoriesToCustomerDto
    {
        public int Customer_id { get; set; }
        public List<int> Category_ids { get; set; } = new List<int>();
        public decimal? Default_discount_percent { get; set; }
    }
}
