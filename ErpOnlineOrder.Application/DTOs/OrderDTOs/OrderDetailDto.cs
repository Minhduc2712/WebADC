using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.DTOs.OrderDTOs
{
    public class OrderDetailDto
    {
        public int Product_id { get; set; }
        public int Quantity { get; set; }
        public decimal Unit_price { get; set; }
    }
}