using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.DTOs
{
    public class OrderDTO
    {
        public int Id { get; set; }
        public string Order_code { get; set; } = null!;
        public DateTime Order_date { get; set; }
        public decimal Total_price { get; set; }
        public string Order_status { get; set; } = null!;
        public string Customer_name { get; set; } = null!;
        public string? Shipping_address { get; set; }
        public string? note { get; set; }
        public List<OrderDetailDTO> Order_details { get; set; } = new();
    }

    public class OrderDetailDTO
    {
        public int Product_id { get; set; }
        public string Product_name { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Unit_price { get; set; }
        public decimal Total_price { get; set; }
    }
}