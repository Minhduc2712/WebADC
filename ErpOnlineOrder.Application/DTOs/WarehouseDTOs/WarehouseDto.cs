namespace ErpOnlineOrder.Application.DTOs.WarehouseDTOs
{
    public class WarehouseDto
    {
        public int Id { get; set; }
        public string Warehouse_code { get; set; } = null!;
        public string Warehouse_name { get; set; } = null!;
        public string Warehouse_address { get; set; } = null!;
        public int Province_id { get; set; }
        public string? Province_name { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
    }
}
