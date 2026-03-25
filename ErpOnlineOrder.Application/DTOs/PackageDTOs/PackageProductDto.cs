namespace ErpOnlineOrder.Application.DTOs.PackageDTOs
{
    public class PackageProductDto
    {
        public int Id { get; set; }
        public int Package_id { get; set; }
        public int Product_id { get; set; }
        public string Product_code { get; set; } = string.Empty;
        public string Product_name { get; set; } = string.Empty;
        public decimal Original_price { get; set; }
        public bool Is_active { get; set; }
    }
}
