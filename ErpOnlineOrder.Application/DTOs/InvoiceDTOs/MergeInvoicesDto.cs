namespace ErpOnlineOrder.Application.DTOs.InvoiceDTOs
{
    public class MergeInvoicesDto
    {
        public List<int> Invoice_ids { get; set; } = new();
        public int Customer_id { get; set; }
        public int Staff_id { get; set; }
        public string? Note { get; set; }
    }
}
