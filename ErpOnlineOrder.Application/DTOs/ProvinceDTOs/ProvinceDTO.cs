using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.DTOs.ProvinceDTOs
{
    public class ProvinceDTO
    {
        public int Id { get; set; }
        public string Province_code { get; set; } = string.Empty;
        public string Province_name { get; set; } = string.Empty;
        public int Region_id { get; set; }
        public string Region_name { get; set; } = string.Empty;
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
    }
}