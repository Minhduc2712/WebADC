using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ErpOnlineOrder.Domain.Models
{
    public class Package
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Package_code { get; set; } = string.Empty;
        [Required]
        [MaxLength(200)]
        public string Package_name { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? Description { get; set; }

        public int? Organization_information_id { get; set; }
        [ForeignKey("Organization_information_id")]
        public virtual Organization_information? Organization_information { get; set; }
        public int? Region_id { get; set; }
        [ForeignKey("Region_id")]
        public virtual Region? Region { get; set; }
        public int? Province_id { get; set; }
        [ForeignKey("Province_id")]
        public virtual Province? Province { get; set; }
        public int? Ward_id { get; set; }
        [ForeignKey("Ward_id")]
        public virtual Ward? Ward { get; set; }
        public bool Is_active { get; set; } = true;
        public int Created_by { get; set; }
        public DateTime Created_at { get; set; }
        public int Updated_by { get; set; }
        public DateTime Updated_at { get; set; }
        public bool Is_deleted { get; set; }
        public virtual ICollection<Package_product> Package_products { get; set; } = new List<Package_product>();
    }
}
