using System.Collections.Generic;

namespace ErpOnlineOrder.Application.DTOs.AuthDTOs
{
    public class DebugUserPermissionsResultDto
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public int TotalUserRoles { get; set; }
        public int ActiveUserRolesCount { get; set; }
        public List<DebugRoleDetailDto> RoleDetails { get; set; } = new();
        public List<string> AllPermissionCodes { get; set; } = new();
        public int PermissionCount { get; set; }
        public bool HasStaffView { get; set; }
        public bool HasProductView { get; set; }
        public bool HasOrderView { get; set; }
    }

    public class DebugRoleDetailDto
    {
        public int UserRoleId { get; set; }
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public bool RoleIsDeleted { get; set; }
        public int RolePermissionsCount { get; set; }
        public List<string> ActiveRolePermissions { get; set; } = new();
    }
}
