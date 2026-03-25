using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ErpOnlineOrder.Domain.Models
{
    public class Organization
    {
        public int Id { get; set; }

        [Required]
        public string Organization_code { get; set; } = string.Empty;

        [Required]
        public string Organization_name { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string Tax_number { get; set; } = string.Empty;
        public int Created_by { get; set; }

        public DateTime Created_at { get; set; }

        public int Updated_by { get; set; }

        public DateTime Updated_at { get; set; }

        public bool Is_deleted { get; set; }

        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    }
}