using System.Collections.Generic;

namespace ErpOnlineOrder.Application.DTOs.AuthDTOs
{
    public class DebugRolePermissionsResultDto
    {
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public bool IsDeleted { get; set; }
        public int PermissionCount { get; set; }
        public List<string> Permissions { get; set; } = new();
    }
}
