using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Application.DTOs.CustomerDTOs
{

    public class CustomerOrgUpdateRequestDto
    {
        [Required]
        public int Customer_id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đơn vị mong muốn")]
        [MaxLength(255, ErrorMessage = "Tên đơn vị không được vượt quá 255 ký tự")]
        public string Organization_name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Organization_code { get; set; }

        [MaxLength(20)]
        public string? Tax_number { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(1000, ErrorMessage = "Ghi chú không được vượt quá 1000 ký tự")]
        public string? Note { get; set; }
    }
}
