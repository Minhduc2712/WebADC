namespace ErpOnlineOrder.Application.DTOs.AuthDTOs
{
    public class SeedAdminResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Hash { get; set; }
        public bool Verified { get; set; }
    }
}
