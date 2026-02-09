using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.DTOs
{
    public class CustomerDTO
    {
        public int Id { get; set; }
        public string Customer_code { get; set; } = null!;
        public string Full_name { get; set; } = null!;
        public string Phone_number { get; set; } = null!;
        public string Address { get; set; } = null!;
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
    }
}