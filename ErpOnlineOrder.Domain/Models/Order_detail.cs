using System.ComponentModel.DataAnnotations.Schema;

namespace ErpOnlineOrder.Domain.Models
{
    public class Order_detail
    {
        public int Id { get; set; }
        public int Order_id { get; set; }
        [ForeignKey("Order_id")]
        public virtual Order? Order { get; set; }
        public int Product_id { get; set; }
        [ForeignKey("Product_id")]
        public virtual Product? Product { get; set; }
        public decimal Tax_rate { get; set; }
        public int Quantity { get; set; }
        public decimal Unit_price { get; set; }
        public decimal Total_price { get; set; }
        public int Created_by { get; set; }
        public DateTime Created_at { get; set; }
        public int Updated_by { get; set; }
        public DateTime Updated_at { get; set; }
        public bool Is_deleted { get; set; }
    }
}
