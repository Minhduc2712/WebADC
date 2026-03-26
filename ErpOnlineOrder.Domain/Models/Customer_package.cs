using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ErpOnlineOrder.Domain.Models
{
    public class Customer_package
    {
        public int Id { get; set; }

        [Required]
        public int Customer_id { get; set; }
        [ForeignKey("Customer_id")]
        public virtual Customer? Customer { get; set; }

        [Required]
        public int Package_id { get; set; }
        [ForeignKey("Package_id")]
        public virtual Package? Package { get; set; }

        public bool Is_active { get; set; } = true;

        public int Created_by { get; set; }
        public DateTime Created_at { get; set; }
        public int Updated_by { get; set; }
        public DateTime Updated_at { get; set; }
        public bool Is_deleted { get; set; }
    }
}
