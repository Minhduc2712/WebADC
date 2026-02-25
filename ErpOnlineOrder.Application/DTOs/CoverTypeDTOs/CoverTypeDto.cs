namespace ErpOnlineOrder.Application.DTOs.CoverTypeDTOs
{
    public class CoverTypeDto : Application.DTOs.IRecordPermissionDto
    {
        public int Id { get; set; }
        public string Cover_type_code { get; set; } = null!;
        public string Cover_type_name { get; set; } = null!;
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
        public bool AllowUpdate { get; set; }
        public bool AllowDelete { get; set; }
    }
}
