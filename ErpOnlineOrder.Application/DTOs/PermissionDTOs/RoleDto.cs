namespace ErpOnlineOrder.Application.DTOs.PermissionDTOs
{
    public class RoleDto
    {
        public int Id { get; set; }
        public string Role_name { get; set; } = string.Empty;
        public int Permission_count { get; set; }
        public DateTime Created_at { get; set; }
    }
}
