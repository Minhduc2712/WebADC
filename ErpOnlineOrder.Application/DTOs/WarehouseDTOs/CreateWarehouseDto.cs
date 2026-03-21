namespace ErpOnlineOrder.Application.DTOs.WarehouseDTOs
{
    public class CreateWarehouseDto
    {
        public string Warehouse_code { get; set; } = null!;
        public string Warehouse_name { get; set; } = null!;
        public string Warehouse_address { get; set; } = null!;
        public string? Warehouse_phone { get; set; }
        public string? Warehouse_email { get; set; }
        public int Province_id { get; set; }
    }
}
