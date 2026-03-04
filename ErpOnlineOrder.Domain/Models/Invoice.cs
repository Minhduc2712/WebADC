using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ErpOnlineOrder.Domain.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        [Required]
        public string Invoice_code { get; set; } = string.Empty;
        
        public DateTime Invoice_date { get; set; }
        
        public int Customer_id { get; set; }
        [ForeignKey("Customer_id")]
        public virtual Customer? Customer { get; set; }
        
        public int Staff_id { get; set; }
        [ForeignKey("Staff_id")]  
        public virtual Staff? Staff { get; set; }
        
        public int? Order_id { get; set; }
        [ForeignKey("Order_id")]
        public virtual Order? Order { get; set; }
        
        public int? Warehouse_export_id { get; set; }
        [ForeignKey("Warehouse_export_id")]
        public virtual Warehouse? Warehouse_export { get; set; }
        
        public decimal Total_amount { get; set; }
        public decimal Tax_amount { get; set; }
        public string Status { get; set; } = "Draft";
        public int? Parent_invoice_id { get; set; }
        [ForeignKey("Parent_invoice_id")]
        public virtual Invoice? Parent_invoice { get; set; }
        public int? Merged_into_invoice_id { get; set; }
        [ForeignKey("Merged_into_invoice_id")]
        public virtual Invoice? Merged_into_invoice { get; set; }
        public string? Split_merge_note { get; set; }
        
        public int Created_by { get; set; }
        public DateTime Created_at { get; set; }
        public int Updated_by { get; set; }
        public DateTime Updated_at { get; set; }
        public bool Is_deleted { get; set; }

        public virtual ICollection<Invoice_detail> Invoice_Details { get; set; } = new List<Invoice_detail>();
        public virtual ICollection<Invoice> Child_invoices { get; set; } = new List<Invoice>();
        public virtual ICollection<Invoice> Merged_invoices { get; set; } = new List<Invoice>();
    }
}
