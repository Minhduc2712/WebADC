using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Domain.Models
{
    public class Customer
    {
        public int Id { get; set; }
        [Required]
        public string Customer_code { get; set; } = string.Empty;
        public string? Full_name { get; set; }
        public string? Phone_number { get; set; }
        public string? Address { get; set; }
        public int Created_by { get; set; }
        public DateTime Created_at { get; set; }
        public int Updated_by { get; set; }
        public DateTime Updated_at { get; set; }
        public bool Is_deleted { get; set; }
        [Required]
        public int User_id { get; set; }
        [ForeignKey("User_id")]
        public virtual User? User { get; set; } = null!;
        public virtual ICollection<Organization_information> Organization_informations { get; set; } = new List<Organization_information>();
        public virtual ICollection<Customer_management> Customer_managements { get; set; } = new List<Customer_management>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public virtual ICollection<Customer_product> Customer_Products { get; set; } = new List<Customer_product>();
        public virtual ICollection<Customer_category> Customer_Categories { get; set; } = new List<Customer_category>();
    }
}
