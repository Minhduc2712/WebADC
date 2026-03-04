using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ErpOnlineOrder.Domain.Models
{
    public class Warehouse_export
    {
        public int Id { get; set; }
        
        [Required]
        public string Warehouse_export_code { get; set; } = string.Empty;
        
        public int Warehouse_id { get; set; }
        [ForeignKey("Warehouse_id")]
        public virtual Warehouse? Warehouse { get; set; }
        
        public int? Order_id { get; set; }
        [ForeignKey("Order_id")]
        public virtual Order? Order { get; set; }
        [Required]
        public int Invoice_id { get; set; }
        [ForeignKey("Invoice_id")]
        public virtual Invoice? Invoice { get; set; }
        
        public int Staff_id { get; set; }
        [ForeignKey("Staff_id")]
        public virtual Staff? Staff { get; set; }
        
        public int Customer_id { get; set; }
        [ForeignKey("Customer_id")]
        public virtual Customer? Customer { get; set; }
        
        public DateTime Export_date { get; set; }
        
        public string? Carrier_name { get; set; }
        public string? Tracking_number { get; set; }
        public string Delivery_status { get; set; } = "Pending";
        public string Status { get; set; } = "Draft";
        public int? Parent_export_id { get; set; }
        [ForeignKey("Parent_export_id")]
        public virtual Warehouse_export? Parent_export { get; set; }
        public int? Merged_into_export_id { get; set; }
        [ForeignKey("Merged_into_export_id")]
        public virtual Warehouse_export? Merged_into_export { get; set; }
        public string? Split_merge_note { get; set; }
        
        public int Created_by { get; set; }
        public DateTime Created_at { get; set; }
        public int Updated_by { get; set; }
        public DateTime Updated_at { get; set; }
        public bool Is_deleted { get; set; }

        public virtual ICollection<Warehouse_export_detail> Warehouse_Export_Details { get; set; } = new List<Warehouse_export_detail>();
        public virtual ICollection<Warehouse_export> Child_exports { get; set; } = new List<Warehouse_export>();
        public virtual ICollection<Warehouse_export> Merged_exports { get; set; } = new List<Warehouse_export>();
    }
}
