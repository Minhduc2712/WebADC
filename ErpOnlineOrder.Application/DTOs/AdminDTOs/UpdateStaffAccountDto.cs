using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Application.DTOs.AdminDTOs
{
    public class UpdateStaffAccountDto
    {
        [Required(ErrorMessage = "User ID là bắt buộc.")]
        public int User_id { get; set; }

        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ.")]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(100, ErrorMessage = "Họ tên không được quá 100 ký tự.")]
        public string? Full_name { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
        [StringLength(20)]
        public string? Phone_number { get; set; }

        public bool? Is_active { get; set; }

        public List<int>? Role_ids { get; set; }

        public int? Province_id { get; set; }

        public int? Ward_id { get; set; }
    }
}