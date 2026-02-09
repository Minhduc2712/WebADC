namespace ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs
{
    public class SplitExportResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public WarehouseExportDto? Original_export { get; set; }
        public List<WarehouseExportDto> New_exports { get; set; } = new();
        public List<int> New_invoice_ids { get; set; } = new();
    }
    public class MergeExportResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public WarehouseExportDto? Merged_export { get; set; }
        public List<int> Merged_export_ids { get; set; } = new();
        public int? Merged_invoice_id { get; set; }
    }
}
