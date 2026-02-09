using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ErpOnlineOrder.Domain.Models
{
    public class Customer_product
    {
        public int Id { get; set; }

        [Required]
        public int Customer_id { get; set; }

        [ForeignKey("Customer_id")]
        public virtual Customer? Customer { get; set; }

        [Required]
        public int Product_id { get; set; }

        [ForeignKey("Product_id")]
        public virtual Product? Product { get; set; }
        public decimal? Custom_price { get; set; }
        public decimal? Discount_percent { get; set; }
        public int? Max_quantity { get; set; }
        public bool Is_active { get; set; } = true;

        public int Created_by { get; set; }

        public DateTime Created_at { get; set; }

        public int Updated_by { get; set; }

        public DateTime Updated_at { get; set; }

        public bool Is_deleted { get; set; }
    }
}
