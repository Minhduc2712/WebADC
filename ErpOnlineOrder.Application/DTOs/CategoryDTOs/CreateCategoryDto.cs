using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.DTOs
{
    public class CreateCategoryDto
    {
        public string Category_code { get; set; } = null!;
        public string Category_name { get; set; } = null!;
    }
}