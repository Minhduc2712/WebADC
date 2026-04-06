using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;
using ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs;
using ErpOnlineOrder.Application.PrintTemplates.Invoice;
using ErpOnlineOrder.Application.PrintTemplates.Order;
using ErpOnlineOrder.Application.PrintTemplates.WarehouseExport;

namespace ErpOnlineOrder.Application.PrintTemplates
{

    public static class PrintTemplateRegistry
    {
        // ── Hóa đơn ──────────────────────────────────────────────────────
        public static IReadOnlyList<IPrintTemplate<InvoiceDto>> InvoiceTemplates { get; } =
            new List<IPrintTemplate<InvoiceDto>>
            {
                new InvoiceStandardTemplate(),      // Mẫu chuẩn đầy đủ
                new InvoiceSimplifiedTemplate(),    // Mẫu rút gọn
            };

        // ── Phiếu xuất kho ────────────────────────────────────────────────
        public static IReadOnlyList<IPrintTemplate<WarehouseExportDto>> ExportTemplates { get; } =
            new List<IPrintTemplate<WarehouseExportDto>>
            {
                new ExportStandardTemplate(),       // Mẫu phiếu xuất kho chuẩn
            };

        // ── Đơn hàng ─────────────────────────────────────────────────────
        public static IReadOnlyList<IPrintTemplate<OrderDTO>> OrderTemplates { get; } =
            new List<IPrintTemplate<OrderDTO>>
            {
                new OrderStandardTemplate(),        // Mẫu xác nhận đơn hàng
            };

        // ── Lookup helpers ────────────────────────────────────────────────
        public static IPrintTemplate<InvoiceDto> GetInvoiceTemplate(string? name) =>
            InvoiceTemplates.FirstOrDefault(t => t.Name == name) ?? InvoiceTemplates[0];

        public static IPrintTemplate<WarehouseExportDto> GetExportTemplate(string? name) =>
            ExportTemplates.FirstOrDefault(t => t.Name == name) ?? ExportTemplates[0];

        public static IPrintTemplate<OrderDTO> GetOrderTemplate(string? name) =>
            OrderTemplates.FirstOrDefault(t => t.Name == name) ?? OrderTemplates[0];
    }
}
