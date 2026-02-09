using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.DTOs
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public string Product_code { get; set; } = null!;
        public string Product_name { get; set; } = null!;
        public string Product_description { get; set; } = null!;
        public string Product_price { get; set; } = null!;
        public string Product_link { get; set; } = null!;
        public string Publisher_name { get; set; } = null!;
        public List<string> Authors { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public List<string> Images { get; set; } = new();
    }
}