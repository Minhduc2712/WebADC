namespace ErpOnlineOrder.Application.DTOs.PermissionDTOs
{
    public class AssignPermissionsToRoleDto
    {
        public int Role_id { get; set; }
        public List<int> Permission_ids { get; set; } = new();
    }
}
