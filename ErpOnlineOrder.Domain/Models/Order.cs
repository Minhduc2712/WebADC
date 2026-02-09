using System.ComponentModel.DataAnnotations.Schema;

namespace ErpOnlineOrder.Domain.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string Order_code { get; set; } = string.Empty;

        public DateTime Order_date { get; set; }

        public int Total_amount { get; set; }

        public decimal Total_price { get; set; }

        public string? Order_status { get; set; }

        public string? Shipping_address { get; set; }

        public string? note { get; set; }

        public int Created_by { get; set; }
        public DateTime Created_at { get; set; }
        public int Updated_by { get; set; }
        public DateTime Updated_at { get; set; }
        public bool Is_deleted { get; set; }
        public int Customer_id { get; set; }
        [ForeignKey("Customer_id")]
        public  virtual Customer? Customer { get; set; }

        public virtual ICollection<Order_detail> Order_Details { get; set; } = new List<Order_detail>();

        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    }
}
