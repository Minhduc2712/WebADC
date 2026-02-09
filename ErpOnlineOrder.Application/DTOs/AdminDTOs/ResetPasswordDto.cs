namespace ErpOnlineOrder.Application.DTOs.AdminDTOs
{
    public class ResetPasswordDto
    {
        public int User_id { get; set; }
        public string New_password { get; set; } = string.Empty;
    }
}
