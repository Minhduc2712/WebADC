using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.DTOs.OrganizationDTOs
{
    public class OrganizationDTO
    {
        public int Id { get; set; }
        public string Organization_code { get; set; } = string.Empty;
        public string Organization_name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int Tax_number { get; set; }
        public string? Recipient_name { get; set; }
        public int Recipient_phone { get; set; }
        public string? Recipient_address { get; set; }
        public int Customer_id { get; set; }
        public string Customer_name { get; set; } = string.Empty;
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
    }
}