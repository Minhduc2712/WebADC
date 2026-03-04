namespace ErpOnlineOrder.Application.DTOs.AdminDTOs
{
    public class CreateStaffAccountDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Full_name { get; set; } = string.Empty;
        public string? Phone_number { get; set; }
        public List<int>? Role_ids { get; set; }
    }
}
