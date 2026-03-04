using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Domain.Models
{
    public class Cover_type
    {
        public int Id { get; set; }

        [Required]
        public string Cover_type_code { get; set; }

        [Required]
        public string Cover_type_name { get; set; }

        public int Created_by { get; set; }

        public DateTime Created_at { get; set; }

        public int Updated_by { get; set; }

        public DateTime Updated_at { get; set; }

        public bool Is_deleted { get; set; }

        public virtual ICollection<Product>? Products { get; set; }
    }
}
