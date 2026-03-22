using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.DTOs.CustomerDTOs
{
    public class UpdateOrganizationByCustomerDto
    {
        [Required]
        public int Customer_id { get; set; }

        [Required(ErrorMessage = "Tên đơn vị là bắt buộc")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Tên đơn vị từ 2 đến 200 ký tự")]
        public string Organization_name { get; set; } = null!;

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        public string Address { get; set; } = null!;

        public string Tax_number { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên người nhận là bắt buộc")]
        public string Recipient_name { get; set; } = null!;

        public string Recipient_phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ nhận hàng là bắt buộc")]
        public string Recipient_address { get; set; } = null!;
    }
}