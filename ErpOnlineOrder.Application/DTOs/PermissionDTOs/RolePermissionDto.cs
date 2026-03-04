namespace ErpOnlineOrder.Application.DTOs.PermissionDTOs
{
    public class RolePermissionDto
    {
        public int Role_id { get; set; }
        public string Role_name { get; set; } = string.Empty;
        public List<PermissionDto> Permissions { get; set; } = new();
    }
}
