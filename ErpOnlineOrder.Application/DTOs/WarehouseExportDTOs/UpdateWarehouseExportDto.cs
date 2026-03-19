namespace ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs
{
    public class UpdateWarehouseExportDto
    {
        public int Warehouse_id { get; set; }
        public DateTime Export_date { get; set; }
        public DateTime? Arrival_date { get; set; }
        public string? Split_merge_note { get; set; }
    }
}
