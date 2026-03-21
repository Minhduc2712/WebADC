using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.DTOs
{
    public class CustomerManagementDTO : IRecordPermissionDto
    {
        public int Id { get; set; }
        public string Staff_name { get; set; } = null!;
        public string Customer_name { get; set; } = null!;
        public string Province_name { get; set; } = null!;
        public string? Ward_name { get; set; }
        public int? Ward_id { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
        public bool AllowUpdate { get; set; }
        public bool AllowDelete { get; set; }
    }
}