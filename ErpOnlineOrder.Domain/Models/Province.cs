using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Domain.Models
{
    public class Province
    {
        public int Id { get; set; }

        [Required]
        public string Province_code { get; set; }

        [Required]
        public string Province_name { get; set; }

        [Required]
        public int Region_id { get; set; }

        public int Created_by { get; set; }

        public DateTime Created_at { get; set; }

        public int Updated_by { get; set; }

        public DateTime Updated_at { get; set; }

        public bool Is_deleted { get; set; }

        public virtual Region Region { get; set; } = null!;

        public virtual ICollection<Customer_management> Customer_managements { get; set; } = new List<Customer_management>();
    }
}
