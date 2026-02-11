namespace ErpOnlineOrder.Application.DTOs.AuthDTOs
{
    public class GrantAllPermissionsResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
    }
}
