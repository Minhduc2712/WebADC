using System.ComponentModel.DataAnnotations.Schema;

namespace ErpOnlineOrder.Domain.Models
{
    public class Product_image
    {
        public int Id { get; set; }

        public string image_url { get; set; } = string.Empty;

        public bool Is_primary { get; set; }

        public int Sort_order { get; set; }

        public int Created_by { get; set; }

        public DateTime Created_at { get; set; }

        public int Updated_by { get; set; }

        public DateTime Updated_at { get; set; }

        public bool Is_deleted { get; set; }

        public int Product_id { get; set; }
        [ForeignKey("Product_id")]
        public virtual Product? Product { get; set; }
    }
}
