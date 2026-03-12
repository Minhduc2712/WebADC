namespace ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs
{
    public class CreateWarehouseExportDto
    {
        public int Invoice_id { get; set; }

        public int Warehouse_id { get; set; }
        public int Staff_id { get; set; }
        public DateTime? Export_date { get; set; }
        public List<CreateWarehouseExportDetailDto>? Details { get; set; }
    }

    public class CreateWarehouseExportDetailDto
    {
        public int Product_id { get; set; }
        public int Quantity_shipped { get; set; }
        public decimal Unit_price { get; set; }
    }
}
