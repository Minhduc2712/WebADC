using System.Collections.Generic;

namespace ErpOnlineOrder.Application.DTOs.InvoiceDTOs
{
    public class CustomerInvoiceRequestDto
    {
        public int WarehouseExportId { get; set; }
        public string? Note { get; set; }
        public List<CustomerInvoiceSplitPartDto>? SplitParts { get; set; }
    }

    public class CustomerInvoiceSplitPartDto
    {
        public List<CustomerInvoiceSplitItemDto> Items { get; set; } = new();
    }

    public class CustomerInvoiceSplitItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}