using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.DTOs.OrganizationDTOs
{
    public class OrganizationDTO : Application.DTOs.IRecordPermissionDto
    {
        public int Id { get; set; }
        public string Organization_code { get; set; } = string.Empty;
        public string Organization_name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Tax_number { get; set; } = string.Empty;
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
        public bool AllowUpdate { get; set; }
        public bool AllowDelete { get; set; }
    }
}