using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Application.DTOs.AuthDTOs
{
    public class RegisterCustomerAccountStepDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [Display(Name = "Tên đăng nhập")]
        [StringLength(100, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu từ 6 đến 100 ký tự")]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu nhập lại không khớp")]
        [Display(Name = "Nhập lại mật khẩu")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class RegisterCustomerPersonalStepDto
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100, ErrorMessage = "Họ tên không được quá 100 ký tự")]
        public string Full_name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được quá 20 ký tự")]
        public string Phone_number { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ cá nhân")]
        [StringLength(200, ErrorMessage = "Địa chỉ không được quá 200 ký tự")]
        public string Address { get; set; } = string.Empty;

        public string? Recipient_name { get; set; }

        public string? Recipient_phone { get; set; }

        public string? Recipient_address { get; set; }

        /// <summary>Tỉnh/thành phố — dùng để tự động gán cán bộ phụ trách</summary>
        public int? Province_id { get; set; }

        /// <summary>Phường/xã — kết hợp với Province_id để tự động gán cán bộ phụ trách chính xác hơn</summary>
        public int? Ward_id { get; set; }
    }

    public class RegisterCustomerOrganizationStepDto : IValidatableObject
    {
        /// <summary>true = khách hàng tự khai báo đơn vị mới; false = chọn đơn vị có sẵn</summary>
        public bool IsNewOrganization { get; set; }

        /// <summary>ID đơn vị có sẵn (dùng khi IsNewOrganization = false)</summary>
        public int Organization_information_id { get; set; }

        // --- Các trường khai báo đơn vị mới (dùng khi IsNewOrganization = true) ---

        [StringLength(50, ErrorMessage = "Mã đơn vị không được quá 50 ký tự")]
        public string? New_Organization_code { get; set; }

        [StringLength(200, ErrorMessage = "Tên đơn vị không được quá 200 ký tự")]
        public string? New_Organization_name { get; set; }

        [StringLength(300, ErrorMessage = "Địa chỉ không được quá 300 ký tự")]
        public string? New_Address { get; set; }

        [StringLength(20, ErrorMessage = "Mã số thuế không được quá 20 ký tự")]
        public string? New_Tax_number { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsNewOrganization)
            {
                if (string.IsNullOrWhiteSpace(New_Organization_name))
                    yield return new ValidationResult("Vui lòng nhập tên đơn vị", new[] { nameof(New_Organization_name) });
                if (string.IsNullOrWhiteSpace(New_Organization_code))
                    yield return new ValidationResult("Vui lòng nhập mã đơn vị", new[] { nameof(New_Organization_code) });
            }
            else
            {
                if (Organization_information_id <= 0)
                    yield return new ValidationResult("Vui lòng chọn đơn vị hợp lệ", new[] { nameof(Organization_information_id) });
            }
        }
    }

    public class FinalizeCustomerRegistrationDto
    {
        [Required]
        public RegisterCustomerAccountStepDto Account { get; set; } = new();

        [Required]
        public RegisterCustomerPersonalStepDto Personal { get; set; } = new();

        [Required]
        public RegisterCustomerOrganizationStepDto Organization { get; set; } = new();
    }
}
