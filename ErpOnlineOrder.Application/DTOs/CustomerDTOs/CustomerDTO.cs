using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.DTOs
{
    public class CustomerDTO : IRecordPermissionDto
    {
        public int Id { get; set; }
        public string Customer_code { get; set; } = null!;
        public string Full_name { get; set; } = null!;
        public string Phone_number { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string? Recipient_name { get; set; }
        public string? Recipient_phone { get; set; }
        public string? Recipient_address { get; set; }
        public int Organization_information_id { get; set; }
        public string Organization_name { get; set; } = string.Empty;
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
        public bool Is_deleted { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public bool AllowUpdate { get; set; }
        public bool AllowDelete { get; set; }
    }
}