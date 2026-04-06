using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Application.DTOs.OrganizationDTOs
{
    public class UpdateOrganizationDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "ID đơn vị không hợp lệ.")]
        public int Id { get; set; }

        [StringLength(50, ErrorMessage = "Mã đơn vị không được vượt quá 50 ký tự.")]
        public string Organization_code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên đơn vị không được để trống.")]
        [StringLength(200, ErrorMessage = "Tên đơn vị không được vượt quá 200 ký tự.")]
        public string Organization_name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ không được để trống.")]
        [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự.")]
        public string Address { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Mã số thuế không được vượt quá 20 ký tự.")]
        public string Tax_number { get; set; } = string.Empty;
    }
}
