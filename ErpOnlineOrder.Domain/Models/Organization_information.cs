using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ErpOnlineOrder.Domain.Models
{
    public class Organization_information
    {
        public int Id { get; set; }

        [Required]
        public string Organization_code { get; set; } = string.Empty;

        [Required]
        public string Organization_name { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public int Tax_number { get; set; }

        public string? Recipient_name { get; set; }

        public int Recipient_phone { get; set; }
         
        public string? Recipient_address { get; set; }

        public int Created_by { get; set; }

        public DateTime Created_at { get; set; }

        public int Updated_by { get; set; }

        public DateTime Updated_at { get; set; }

        public bool Is_deleted { get; set; }

        public int Customer_id { get; set; }

        [ForeignKey("Customer_id")]
        public virtual Customer? Customer { get; set; }
    }
}
