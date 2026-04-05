namespace ErpOnlineOrder.WebMVC.Models
{
    public class InvoiceByOrderViewModel
    {
        public int OrderId { get; set; }
        public string? OrderCode { get; set; }
        public int ExportCount { get; set; }
        public List<int> ExportIds { get; set; } = new();
        public List<AggregatedExportItemViewModel> AggregatedItems { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }

    public class AggregatedExportItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
