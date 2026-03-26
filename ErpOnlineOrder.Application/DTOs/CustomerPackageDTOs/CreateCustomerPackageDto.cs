using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Application.DTOs.CustomerPackageDTOs
{
    public class CreateCustomerPackageDto
    {
        [Required]
        public int Customer_id { get; set; }
        [Required]
        public int Package_id { get; set; }
        public bool Is_active { get; set; } = true;
    }
}