using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Domain.Models
{
    public class Publisher
    {
        public int Id { get; set; }
        
        [Required]
        public string Publisher_code { get; set; } = string.Empty;
        
        [Required]
        public string Publisher_name { get; set; } = string.Empty;
        
        public string? Publisher_address { get; set; }
        
        public string? Publisher_phone { get; set; }
        
        public string? Publisher_email { get; set; }

        public int Created_by { get; set; }
        public DateTime Created_at { get; set; }
        public int Updated_by { get; set; }
        public DateTime Updated_at { get; set; }
        public bool Is_deleted { get; set; }

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
