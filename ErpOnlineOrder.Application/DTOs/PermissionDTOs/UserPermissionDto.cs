namespace ErpOnlineOrder.Application.DTOs.PermissionDTOs
{
    public class UserPermissionDto
    {
        public int User_id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Full_name { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
    }
}
