using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Domain.Models
{
    public class Author
    {
        public int Id { get; set; }

        [Required]
        public string Author_code { get; set; } = string.Empty;

        [Required]
        public string Author_name { get; set; } = string.Empty;

        public string? Pen_name { get; set; }

        public string? Email_author { get; set; }

        public string? Phone_number { get; set; }

        public string? birth_date { get; set; }

        public string? death_date { get; set; }

        public string? Nationality { get; set; }

        public string? Biography { get; set; }

        public int Created_by { get; set; }

        public DateTime Created_at { get; set; }

        public int Updated_by { get; set; }

        public DateTime Updated_at { get; set; }

        public bool Is_deleted { get; set; }

        public virtual ICollection<Product_author> Product_Authors { get; set; } = new List<Product_author>();
    }
}
