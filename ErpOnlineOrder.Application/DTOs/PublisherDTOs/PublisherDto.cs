namespace ErpOnlineOrder.Application.DTOs.PublisherDTOs
{
    public class PublisherDto : Application.DTOs.IRecordPermissionDto
    {
        public int Id { get; set; }
        public string Publisher_code { get; set; } = null!;
        public string Publisher_name { get; set; } = null!;
        public string? Publisher_address { get; set; }
        public string? Publisher_phone { get; set; }
        public string? Publisher_email { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
        public bool AllowUpdate { get; set; }
        public bool AllowDelete { get; set; }
    }
}
