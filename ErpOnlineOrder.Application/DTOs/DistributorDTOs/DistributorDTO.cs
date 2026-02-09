using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.DTOs
{
    public class DistributorDTO
    {
        public int Id { get; set; }
        public string Distributor_code { get; set; } = null!;
        public string Distributor_name { get; set; } = null!;
        public string Distributor_address { get; set; } = null!;
        public string Distributor_phone { get; set; } = null!;
        public string Distributor_email { get; set; } = null!;
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
    }
}