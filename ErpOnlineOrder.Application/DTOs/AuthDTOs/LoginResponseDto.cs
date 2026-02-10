namespace ErpOnlineOrder.Application.DTOs.AuthDTOs
{
    /// <summary>
    /// Dữ liệu trả về từ API Login để MVC set session (hoặc client lưu token).
    /// </summary>
    public class LoginResponseDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string? Email { get; set; }
        public string FullName { get; set; } = null!;
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
        public string? StaffCode { get; set; }
        public string? CustomerCode { get; set; }
        public string UserType { get; set; } = ""; // "Staff" | "Customer"
    }
}
