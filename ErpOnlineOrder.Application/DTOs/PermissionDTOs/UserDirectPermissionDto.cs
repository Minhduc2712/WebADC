namespace ErpOnlineOrder.Application.DTOs.PermissionDTOs
{
    public class UserDirectPermissionDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PermissionId { get; set; }
        public string Permission_code { get; set; } = string.Empty;
        public string Module_name { get; set; } = string.Empty;
        public string Action_name { get; set; } = string.Empty;
        public DateTime GrantedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? Note { get; set; }
        public bool IsValid => ExpiresAt == null || ExpiresAt > DateTime.Now;
    }
    public class AssignPermissionsToUserDto
    {
        public int User_id { get; set; }
        public List<int> Permission_ids { get; set; } = new List<int>();
        public DateTime? ExpiresAt { get; set; }
        
        public string? Note { get; set; }
    }
    public class UserFullPermissionDto
    {
        public int User_id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Full_name { get; set; }
        public List<RolePermissionSummaryDto> RolePermissions { get; set; } = new();
        public List<UserDirectPermissionDto> DirectPermissions { get; set; } = new();
        public List<string> AllPermissionCodes { get; set; } = new();
    }
    public class RolePermissionSummaryDto
    {
        public int Role_id { get; set; }
        public string Role_name { get; set; } = string.Empty;
        public List<string> Permission_codes { get; set; } = new();
    }
}
