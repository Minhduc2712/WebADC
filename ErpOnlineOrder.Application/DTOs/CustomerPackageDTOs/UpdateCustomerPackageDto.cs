using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Application.DTOs.CustomerPackageDTOs
{
    public class UpdateCustomerPackageDto
    {
        [Required]
        public int Id { get; set; }
        public bool Is_active { get; set; } = true;
    }
}