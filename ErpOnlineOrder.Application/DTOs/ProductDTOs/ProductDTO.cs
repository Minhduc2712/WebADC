using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.DTOs
{
    public class ProductDTO : IRecordPermissionDto
    {
        public int Id { get; set; }
        public string Product_code { get; set; } = null!;
        public string Product_name { get; set; } = null!;
        public string Product_description { get; set; } = null!;
        /// <summary>Giá số - dùng cho sắp xếp, tính toán. Format hiển thị tại UI (ToString("N0")).</summary>
        public decimal? Product_price_decimal { get; set; }
        /// <summary>Giá đã format - giữ cho tương thích. Nên dùng Product_price_decimal + format tại UI.</summary>
        public string Product_price { get; set; } = null!;
        public string Product_link { get; set; } = null!;
        public string Publisher_name { get; set; } = null!;
        public List<string> Authors { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public List<string> Images { get; set; } = new();
        public bool AllowUpdate { get; set; }
        public bool AllowDelete { get; set; }
        public bool AllowExport { get; set; }
        public bool AllowImport { get; set; }
    }
}