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

        /// <summary>Tỉnh/thành phố — dùng để tự động gán cán bộ phụ trách</summary>
        public int? Province_id { get; set; }

        /// <summary>Phường/xã — kết hợp với Province_id để tự động gán cán bộ phụ trách chính xác hơn</summary>
        public int? Ward_id { get; set; }
    }

    public class RegisterCustomerOrganizationStepDto
    {
        [Required(ErrorMessage = "Tên đơn vị là bắt buộc")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Tên đơn vị từ 2 đến 200 ký tự")]
        public string Organization_name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ đơn vị là bắt buộc")]
        public string Address { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Mã số thuế không hợp lệ")]
        public string Tax_number { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên người nhận là bắt buộc")]
        public string Recipient_name { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Số điện thoại người nhận không hợp lệ")]
        public string Recipient_phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ nhận hàng là bắt buộc")]
        public string Recipient_address { get; set; } = string.Empty;
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
