namespace ErpOnlineOrder.Application.DTOs.AuthDTOs
{
    public class CheckAdminResultDto
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string? PasswordHash { get; set; }
        public int? PasswordHashLength { get; set; }
        public string? TestPassword { get; set; }
        public bool VerifyResult { get; set; }
    }
}
