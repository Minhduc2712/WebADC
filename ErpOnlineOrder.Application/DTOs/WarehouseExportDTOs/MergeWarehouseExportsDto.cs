namespace ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs
{
    public class MergeWarehouseExportsDto
    {
        public List<int> Export_ids { get; set; } = new();
        public int Warehouse_id { get; set; }
        public int? Merged_invoice_id { get; set; }
        public bool Auto_merge_invoices { get; set; } = true;
        public int Staff_id { get; set; }
        public string? Note { get; set; }
    }
}
