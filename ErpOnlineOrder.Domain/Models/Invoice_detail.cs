using System.ComponentModel.DataAnnotations.Schema;

namespace ErpOnlineOrder.Domain.Models
{
    public class Invoice_detail
    {
        public int Id { get; set; }
        public int Invoice_id { get; set; }
        [ForeignKey("Invoice_id")]
        public virtual Invoice? Invoice { get; set; }
        public int Product_id { get; set; }
        [ForeignKey("Product_id")]
        public virtual Product? Product { get; set; }
        public int Quantity { get; set; }
        public decimal Unit_price { get; set; }
        public decimal Total_price { get; set; }
        public decimal Tax_rate { get; set; }
        public int Created_by { get; set; }

        public DateTime Created_at { get; set; }

        public int Updated_by { get; set; }

        public DateTime Updated_at { get; set; }

        public bool Is_deleted { get; set; }
    }
}
