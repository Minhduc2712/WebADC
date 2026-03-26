namespace ErpOnlineOrder.Application.DTOs.CustomerPackageDTOs
{
    public class CustomerPackageDto : Application.DTOs.IRecordPermissionDto
    {
        public int Id { get; set; }
        public int Customer_id { get; set; }
        public string Customer_name { get; set; } = string.Empty;
        public int Package_id { get; set; }
        public string Package_code { get; set; } = string.Empty;
        public string Package_name { get; set; } = string.Empty;
        public bool Is_active { get; set; }
        public DateTime Created_at { get; set; }
        public bool AllowUpdate { get; set; }
        public bool AllowDelete { get; set; }
    }
}