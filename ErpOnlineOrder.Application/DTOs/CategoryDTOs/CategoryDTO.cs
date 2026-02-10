using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Category_code { get; set; } = null!;
        public string Category_name { get; set; } = null!;
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
    }
}