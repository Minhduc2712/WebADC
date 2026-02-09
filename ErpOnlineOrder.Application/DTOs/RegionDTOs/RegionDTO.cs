using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.DTOs.RegionDTOs
{
    public class RegionDTO
    {
        public int Id { get; set; }
        public string Region_code { get; set; } = string.Empty;
        public string Region_name { get; set; } = string.Empty;
        public int Province_count { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
    }
}