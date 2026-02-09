using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.DTOs.OrderDTOs
{
    public class CreateOrderDto
    {
        public DateTime Order_date { get; set; }
        public int? Customer_id { get; set; }
        /// <summary>Địa chỉ giao hàng (có thể để trống).</summary>
        public string? Shipping_address { get; set; }
        /// <summary>Ghi chú đơn hàng (có thể để trống).</summary>
        public string? note { get; set; }
        public List<OrderDetailDto> Order_details { get; set; } = new();
    }
}