namespace ErpOnlineOrder.Application.DTOs
{
    public class UpdateCategoryDto
    {
        public int Id { get; set; }
        public string Category_code { get; set; } = null!;
        public string Category_name { get; set; } = null!;
    }
}