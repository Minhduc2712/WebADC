using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Domain.Models
{
    public class Warehouse
    {
        public int Id { get; set; }
        [Required]
        public string Warehouse_code { get; set; } = string.Empty;
        [Required]
        public string Warehouse_name { get; set; } = string.Empty;
        [Required]
        public string Warehouse_address { get; set; } = string.Empty;
        public int Created_by { get; set; }
        public DateTime Created_at { get; set; }
        public int Updated_by { get; set; }
        public DateTime Updated_at { get; set; }
        public bool Is_deleted { get; set; }
        [Required]
        public int Province_id { get; set; }
        public virtual Province? Province { get; set; }
        public virtual ICollection<Warehouse_export> Warehouse_Exports { get; set; } = new List<Warehouse_export>();  
        public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();
    }
}
