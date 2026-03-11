using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.DTOs.OrderDTOs
{
    public class OrderDetailDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn sản phẩm")]
        public int Product_id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Giá không được âm")]
        public decimal Unit_price { get; set; }
    }
}