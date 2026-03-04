using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.DTOs.StaffDTOs
{
    public class UpdateStaffDto
    {
        public string User_id { get; set; } = null!;
        public string Staff_code { get; set; } = null!;
        public string Full_name { get; set; } = null!;
        public string Phone_number { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string Staff_email { get; set; } = null!;
    }
}
