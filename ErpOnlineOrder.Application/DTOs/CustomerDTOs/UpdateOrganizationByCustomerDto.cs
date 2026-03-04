using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.DTOs.CustomerDTOs
{
    public class UpdateOrganizationByCustomerDto
    {
        public int Customer_id { get; set; }
        public string Organization_name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public int Tax_number { get; set; }
        public string Recipient_name { get; set; } = null!;
        public int Recipient_phone { get; set; }
        public string Recipient_address { get; set; } = null!;
    }
}