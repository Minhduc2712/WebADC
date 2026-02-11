using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.DTOs.OrderDTOs
{
    public class UpdateOrderDto
    {
        public int Id { get; set; }
        public string Order_code { get; set; } = null!;
        public DateTime Order_date { get; set; }
        public List<OrderDetailDto> Order_details { get; set; } = new();
        /// <summary>User id cập nhật (controller gán từ GetCurrentUserId()).</summary>
        public int Updated_by { get; set; }
    }
}