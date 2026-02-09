using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Domain.Models
{
    public class Distributor
    {
        public int Id { get; set; }
        public string? Distributor_code { get; set; }
        public string? Distributor_name { get; set; }
        public string? Distributor_address { get; set; }
        public string? Distributor_phone { get; set; }
        public string? Distributor_email { get; set; }
        public int Created_by { get; set; }
        public DateTime Created_at { get; set; }
        public int Updated_by { get; set; }
        public DateTime Updated_at { get; set; }
        public bool Is_deleted { get; set; }
        public virtual ICollection<Product>? Products { get; set; }
    }
}
