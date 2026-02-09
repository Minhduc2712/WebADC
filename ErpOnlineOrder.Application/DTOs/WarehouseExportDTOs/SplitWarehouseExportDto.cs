namespace ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs
{
    public class SplitWarehouseExportDto
    {
        public int Source_export_id { get; set; }
        public List<SplitExportPart> Split_parts { get; set; } = new();
        public bool Auto_split_invoice { get; set; } = true;
        public string? Note { get; set; }
    }
    public class SplitExportPart
    {
        public List<SplitExportItem> Items { get; set; } = new();
    }
    public class SplitExportItem
    {
        public int Export_detail_id { get; set; }
        public int Quantity { get; set; }
    }
}
