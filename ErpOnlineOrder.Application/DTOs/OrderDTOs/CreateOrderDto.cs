using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.DTOs.OrderDTOs
{
    public class CreateOrderDto
    {
        public DateTime Order_date { get; set; }
        public int? Customer_id { get; set; }
        public string? Shipping_address { get; set; }
        public string? note { get; set; }

        [MinLength(1, ErrorMessage = "Đơn hàng phải có ít nhất một sản phẩm")]
        public List<OrderDetailDto> Order_details { get; set; } = new();

        public int Created_by { get; set; }
    }
}