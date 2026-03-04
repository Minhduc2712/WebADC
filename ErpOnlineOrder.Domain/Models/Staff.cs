using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Domain.Models
{
    public class Staff
    {
        public int Id { get; set; }

        [Required]
        public string Staff_code { get; set; } = string.Empty;

        public string? Full_name { get; set; }

        public string? Phone_number { get; set; }

        public int Created_by { get; set; }

        public DateTime Created_at { get; set; }

        public int Updated_by { get; set; }

        public DateTime Updated_at { get; set; }

        public bool Is_deleted { get; set; }

        [Required]
        public int User_id { get; set; }

        [ForeignKey("User_id")]
        public virtual User? User { get; set; }

        public virtual ICollection<Customer_management> Customer_managements { get; set; } = new List<Customer_management>();

        // Một nhân viên có thể tạo nhiều hóa đơn
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        
        // Một nhân viên có thể tạo nhiều phiếu xuất kho
        public virtual ICollection<Warehouse_export> Warehouse_exports { get; set; } = new List<Warehouse_export>();
    }
}
