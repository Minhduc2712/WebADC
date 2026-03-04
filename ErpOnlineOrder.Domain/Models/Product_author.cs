using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ErpOnlineOrder.Domain.Models
{
    public class Product_author
    {
        public int Id { get; set; }

        public int Created_by { get; set; }

        public DateTime Created_at { get; set; }

        public int Updated_by { get; set; }

        public DateTime Updated_at { get; set; }

        public bool Is_deleted { get; set; }

        [Required]
        public int Product_id { get; set; }

        [ForeignKey("Product_id")]
        public virtual Product? Product { get; set; }

        [Required]
        public int Author_id { get; set; }

        [ForeignKey("Author_id")]
        public virtual Author? Author { get; set; }
    }
}
