namespace ErpOnlineOrder.Application.DTOs.CoverTypeDTOs
{
    public class UpdateCoverTypeDto
    {
        public int Id { get; set; }
        public string Cover_type_code { get; set; } = null!;
        public string Cover_type_name { get; set; } = null!;
    }
}
