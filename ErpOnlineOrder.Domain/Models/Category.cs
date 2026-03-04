using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Domain.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string Category_code { get; set; } = string.Empty;

        [Required]
        public string Category_name { get; set; } = string.Empty;

        public int Created_by { get; set; }

        public DateTime Created_at { get; set; }

        public int Updated_by { get; set; }

        public DateTime Updated_at { get; set; }

        public bool Is_deleted { get; set; }

        public virtual ICollection<Product_category> Product_Categories { get; set; } = new List<Product_category>();
        public virtual ICollection<Customer_category> Customer_Categories { get; set; } = new List<Customer_category>();
    }
}
