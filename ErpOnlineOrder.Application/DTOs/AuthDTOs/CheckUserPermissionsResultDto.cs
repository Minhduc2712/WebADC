using System.Collections.Generic;

namespace ErpOnlineOrder.Application.DTOs.AuthDTOs
{
    public class CheckUserPermissionsResultDto
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public List<RoleInfoDto> Roles { get; set; } = new();
        public List<string> AllPermissionCodes { get; set; } = new();
        public int PermissionCount { get; set; }
        public bool HasStaffView { get; set; }
        public object? FullPermissions { get; set; }
    }

    public class RoleInfoDto
    {
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public int PermissionCount { get; set; }
    }
}
