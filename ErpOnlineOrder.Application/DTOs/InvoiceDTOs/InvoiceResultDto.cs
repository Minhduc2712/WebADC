namespace ErpOnlineOrder.Application.DTOs.InvoiceDTOs
{
    public class SplitInvoiceResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public InvoiceDto? Original_invoice { get; set; }
        public List<InvoiceDto> New_invoices { get; set; } = new();
    }
    public class MergeInvoiceResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public InvoiceDto? Merged_invoice { get; set; }
        public List<int> Merged_invoice_ids { get; set; } = new();
    }
}
