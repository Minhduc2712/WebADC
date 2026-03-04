using ErpOnlineOrder.Application.DTOs.OrderDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.DTOs.OrderDTOs
{
    public class UpdateOrderResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? Order_id { get; set; }
        public string? Order_code { get; set; }
        public List<ProductValidationError> Invalid_products { get; set; } = new();
    }
}
