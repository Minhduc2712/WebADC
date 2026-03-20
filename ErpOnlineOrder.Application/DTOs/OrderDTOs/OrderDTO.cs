using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.DTOs
{
    public class OrderDTO : IRecordPermissionDto
    {
        public int Id { get; set; }
        public string Order_code { get; set; } = null!;
        public DateTime Order_date { get; set; }
        public decimal Total_price { get; set; }
        public string Order_status { get; set; } = null!;
        public string Customer_name { get; set; } = null!;
        public string? Shipping_address { get; set; }
        public string? note { get; set; }
        public int? Parent_order_id { get; set; }
        public int IndentLevel { get; set; } 
        public List<OrderDetailDTO> Order_details { get; set; } = new();
        public bool AllowUpdate { get; set; }
        public bool AllowDelete { get; set; }
        public bool AllowApprove { get; set; }
        public bool AllowReject { get; set; }
        public bool AllowExport { get; set; }
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