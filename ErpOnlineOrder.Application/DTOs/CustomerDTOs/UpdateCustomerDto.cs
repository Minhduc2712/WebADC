using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.DTOs.CustomerDTOs
{
    public class UpdateCustomerDto
    {
        public string User_id { get; set; } = null!;
        public string Customer_code { get; set; } = null!;
        public string Full_name { get; set; } = null!;
        public string Phone_number { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string Customer_email { get; set; } = null!;

    }
}
