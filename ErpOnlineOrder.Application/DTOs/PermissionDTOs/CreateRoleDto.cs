namespace ErpOnlineOrder.Application.DTOs.PermissionDTOs
{
    public class CreateRoleDto
    {
        public string Role_name { get; set; } = string.Empty;
        public List<int>? Permission_ids { get; set; }
    }
}
