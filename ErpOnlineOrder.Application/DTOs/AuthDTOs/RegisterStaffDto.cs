using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.DTOs.AuthDTOs
{
    public class RegisterStaffDto
    {
        public int Created_id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Full_name { get; set; }
        public string Phone_number { get; set; }
        public string Role_type { get; set; }
    }
}
