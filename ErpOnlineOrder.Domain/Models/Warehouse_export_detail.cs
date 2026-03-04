using System.ComponentModel.DataAnnotations.Schema;

namespace ErpOnlineOrder.Domain.Models
{
    public class Warehouse_export_detail
    {
        public int Id { get; set; }

        public int Warehouse_export_id { get; set; }
        [ForeignKey("Warehouse_export_id")]
        public virtual Warehouse_export? Warehouse_export { get; set; }

        public int Warehouse_id { get; set; }
        [ForeignKey("Warehouse_id")]
        public virtual Warehouse? Warehouse { get; set; }

        public int Product_id { get; set; }
        [ForeignKey("Product_id")]
        public virtual Product? Product { get; set; }
        public int Quantity_shipped { get; set; }
        public decimal Unit_price { get; set; }
        public decimal Total_price { get; set; }

        public int Created_by { get; set; }
        public DateTime Created_at { get; set; }
        public int Updated_by { get; set; }
        public DateTime Updated_at { get; set; }
        public bool Is_deleted { get; set; }
    }
}
