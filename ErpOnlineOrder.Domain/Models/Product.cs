using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Domain.Models
{
    public class Product
    {
        public int Id { get; set; }
        
        [Required]
        public string Product_code { get; set; } = string.Empty;
        
        [Required]
        public string Product_name { get; set; } = string.Empty;
        
        public string? Product_price { get; set; }
        
        public string? Product_link { get; set; }
        
        public string? Product_description { get; set; }
        
        public decimal? Tax_rate { get; set; }
        
        public int Created_by { get; set; }
        
        public DateTime Created_at { get; set; }
        
        public int Updated_by { get; set; }
        
        public DateTime Updated_at { get; set; }
        
        public bool Is_deleted { get; set; }
        
        public int? Cover_type_id { get; set; }
        
        [ForeignKey("Cover_type_id")]
        public virtual Cover_type? Cover_type { get; set; }
        
        public int? Publisher_id { get; set; }
        
        [ForeignKey("Publisher_id")]
        public virtual Publisher? Publisher { get; set; }
        
        public int? Distributor_id { get; set; }
        
        [ForeignKey("Distributor_id")]
        public virtual Distributor? Distributor { get; set; }
        
        public virtual ICollection<Product_author> Product_Authors { get; set; } = new List<Product_author>();
        
        public virtual ICollection<Product_image> Product_Images { get; set; } = new List<Product_image>();
        
        public virtual ICollection<Product_category> Product_Categories { get; set; } = new List<Product_category>();
        
        public virtual ICollection<Invoice_detail> Invoice_Details { get; set; } = new List<Invoice_detail>(); 
        
        public virtual ICollection<Order_detail> Order_Details { get; set; } = new List<Order_detail>();
        
        public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();
        public virtual ICollection<Customer_product> Customer_Products { get; set; } = new List<Customer_product>();
    }
}
