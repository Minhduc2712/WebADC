using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Application.DTOs.CustomerDTOs
{
    public class UpdateOrganizationByCustomerDto
    {
        [Required]
        public int Customer_id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn đơn vị")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn đơn vị hợp lệ")]
        public int Organization_information_id { get; set; }

        public string? Recipient_name { get; set; }

        public string? Recipient_phone { get; set; }

        public string? Recipient_address { get; set; }
    }
}