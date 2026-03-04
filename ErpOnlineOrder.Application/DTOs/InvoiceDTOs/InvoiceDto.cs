namespace ErpOnlineOrder.Application.DTOs.InvoiceDTOs
{
    public class InvoiceDto : Application.DTOs.IRecordPermissionDto
    {
        public int Id { get; set; }
        public string Invoice_code { get; set; } = string.Empty;
        public DateTime Invoice_date { get; set; }
        public int Customer_id { get; set; }
        public string Customer_name { get; set; } = string.Empty;
        public int? Province_id { get; set; }
        public string? Province_name { get; set; }
        public decimal Total_amount { get; set; }
        public decimal Tax_amount { get; set; }
        public decimal Grand_total => Total_amount + Tax_amount;
        public string Status { get; set; } = string.Empty;
        public int? Parent_invoice_id { get; set; }
        public string? Parent_invoice_code { get; set; }
        public List<InvoiceDetailDto> Details { get; set; } = new();
        public bool AllowUpdate { get; set; }
        public bool AllowDelete { get; set; }
    }
    public class InvoiceDetailDto
    {
        public int Id { get; set; }
        public int Product_id { get; set; }
        public string Product_name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Unit_price { get; set; }
        public decimal Total_price { get; set; }
        public decimal Tax_rate { get; set; }
    }
}