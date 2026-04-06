namespace ErpOnlineOrder.Application.DTOs.AdminDTOs
{
    public class StaffAccountDto : Application.DTOs.IRecordPermissionDto
    {
        public int User_id { get; set; }
        public int Staff_id { get; set; }
        public string Staff_code { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Full_name { get; set; } = string.Empty;
        public string? Phone_number { get; set; }
        public bool Is_active { get; set; }
        public List<string> Roles { get; set; } = new();
        public DateTime Created_at { get; set; }
        public int? Province_id { get; set; }
        public string? Province_name { get; set; }
        public int? Ward_id { get; set; }
        public string? Ward_name { get; set; }
        public bool AllowUpdate { get; set; }
        public bool AllowDelete { get; set; }
        public bool AllowAssign { get; set; }
    }
}
