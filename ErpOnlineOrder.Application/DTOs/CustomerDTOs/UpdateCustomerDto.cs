using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.DTOs.CustomerDTOs
{
    public class UpdateCustomerDto
    {
        [Required(ErrorMessage = "User ID là bắt buộc")]
        public string User_id { get; set; } = null!;

        [Required(ErrorMessage = "Mã khách hàng là bắt buộc")]
        public string Customer_code { get; set; } = null!;

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Họ tên từ 2 đến 200 ký tự")]
        public string Full_name { get; set; } = null!;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone_number { get; set; } = null!;

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        public string Address { get; set; } = null!;

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Customer_email { get; set; } = null!;
    }
}
