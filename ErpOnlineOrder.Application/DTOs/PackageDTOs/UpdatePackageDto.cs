using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Application.DTOs.PackageDTOs
{
    public class UpdatePackageDto
    {
        [Required]
        [MaxLength(200)]
        public string Package_name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int? Organization_information_id { get; set; }
        public int? Region_id { get; set; }
        public int? Province_id { get; set; }
        public int? Ward_id { get; set; }

        public bool Is_active { get; set; } = true;
    }
}
