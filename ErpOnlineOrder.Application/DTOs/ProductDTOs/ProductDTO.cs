using System.Globalization;

namespace ErpOnlineOrder.Application.DTOs
{
    public class ProductDTO : IRecordPermissionDto
    {
        private static readonly CultureInfo ViCulture = CultureInfo.GetCultureInfo("vi-VN");

        public int Id { get; set; }
        public string Product_code { get; set; } = null!;
        public string Product_name { get; set; } = null!;
        public string Product_description { get; set; } = null!;
        public decimal Product_price { get; set; }
        public string Product_price_formatted => Product_price.ToString("N0", ViCulture) + " đ";
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