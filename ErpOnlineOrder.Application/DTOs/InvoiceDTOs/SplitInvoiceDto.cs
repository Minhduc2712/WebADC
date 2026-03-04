namespace ErpOnlineOrder.Application.DTOs.InvoiceDTOs
{
    public class SplitInvoiceDto
    {
        public int Source_invoice_id { get; set; }
        public List<SplitInvoicePart> Split_parts { get; set; } = new();
        public string? Note { get; set; }
    }
    public class SplitInvoicePart
    {
        public List<SplitInvoiceItem> Items { get; set; } = new();
    }
    public class SplitInvoiceItem
    {
        public int Invoice_detail_id { get; set; }
        public int Quantity { get; set; }
    }
}
