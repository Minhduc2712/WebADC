using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Domain.Models
{
    public class Ward
    {
        public int Id { get; set; }

        [Required]
        public string Ward_code { get; set; } = null!;

        [Required]
        public string Ward_name { get; set; } = null!;

        [Required]
        public int Province_id { get; set; }

        public int Created_by { get; set; }

        public DateTime Created_at { get; set; }

        public int Updated_by { get; set; }

        public DateTime Updated_at { get; set; }

        public bool Is_deleted { get; set; }

        public virtual Province Province { get; set; } = null!;

        public virtual ICollection<Customer_management> Customer_managements { get; set; } = new List<Customer_management>();
    }
}
