using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.PrintTemplates;
using QuestPDF.Infrastructure;

namespace ErpOnlineOrder.Application.Services
{
    public class PrintService : IPrintService
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IWarehouseExportService _warehouseExportService;
        private readonly IOrderService _orderService;
        private readonly ISettingService _settingService;

        public PrintService(
            IInvoiceService invoiceService,
            IWarehouseExportService warehouseExportService,
            IOrderService orderService,
            ISettingService settingService)
        {
            _invoiceService = invoiceService;
            _warehouseExportService = warehouseExportService;
            _orderService = orderService;
            _settingService = settingService;
        }

        // ── Template lists ────────────────────────────────────────────────

        public IEnumerable<(string Name, string DisplayName)> GetInvoiceTemplates() =>
            PrintTemplateRegistry.InvoiceTemplates.Select(t => (t.Name, t.DisplayName));

        public IEnumerable<(string Name, string DisplayName)> GetExportTemplates() =>
            PrintTemplateRegistry.ExportTemplates.Select(t => (t.Name, t.DisplayName));

        public IEnumerable<(string Name, string DisplayName)> GetOrderTemplates() =>
            PrintTemplateRegistry.OrderTemplates.Select(t => (t.Name, t.DisplayName));

        // ── Print methods ─────────────────────────────────────────────────

        public async Task<(byte[] Data, string ContentType, string FileName)> PrintInvoiceAsync(
            int id, int userId, string format, string templateName)
        {
            var invoice = await _invoiceService.GetByIdAsync(id, userId)
                ?? throw new KeyNotFoundException($"Không tìm thấy hóa đơn #{id}.");

            if (!PrintFormat.IsValid(format))
                throw new ArgumentException($"Định dạng không hợp lệ: {format}");

            var template = PrintTemplateRegistry.GetInvoiceTemplate(templateName);
            var settings = await BuildSettingsAsync();
            var bytes = format.ToLowerInvariant() switch
            {
                PrintFormat.Pdf  => template.ToPdf(invoice, settings),
                PrintFormat.Docx => template.ToDocx(invoice, settings),
                _                => template.ToXml(invoice, settings),
            };

            var ext = PrintFormat.GetExtension(format);
            var ct  = PrintFormat.GetContentType(format);
            return (bytes, ct, $"HoaDon_{invoice.Invoice_code}{ext}");
        }

        public async Task<(byte[] Data, string ContentType, string FileName)> PrintWarehouseExportAsync(
            int id, int userId, string format, string templateName)
        {
            var export = await _warehouseExportService.GetByIdAsync(id, userId)
                ?? throw new KeyNotFoundException($"Không tìm thấy phiếu xuất kho #{id}.");

            if (!PrintFormat.IsValid(format))
                throw new ArgumentException($"Định dạng không hợp lệ: {format}");

            var template = PrintTemplateRegistry.GetExportTemplate(templateName);
            var settings = await BuildSettingsAsync();
            var bytes = format.ToLowerInvariant() switch
            {
                PrintFormat.Pdf  => template.ToPdf(export, settings),
                PrintFormat.Docx => template.ToDocx(export, settings),
                _                => template.ToXml(export, settings),
            };

            var ext = PrintFormat.GetExtension(format);
            var ct  = PrintFormat.GetContentType(format);
            return (bytes, ct, $"PhieuXuatKho_{export.Warehouse_export_code}{ext}");
        }

        public async Task<(byte[] Data, string ContentType, string FileName)> PrintOrderAsync(
            int id, int userId, string format, string templateName)
        {
            var order = await _orderService.GetByIdAsync(id, userId)
                ?? throw new KeyNotFoundException($"Không tìm thấy đơn hàng #{id}.");

            if (!PrintFormat.IsValid(format))
                throw new ArgumentException($"Định dạng không hợp lệ: {format}");

            var template = PrintTemplateRegistry.GetOrderTemplate(templateName);
            var settings = await BuildSettingsAsync();
            var bytes = format.ToLowerInvariant() switch
            {
                PrintFormat.Pdf  => template.ToPdf(order, settings),
                PrintFormat.Docx => template.ToDocx(order, settings),
                _                => template.ToXml(order, settings),
            };

            var ext = PrintFormat.GetExtension(format);
            var ct  = PrintFormat.GetContentType(format);
            return (bytes, ct, $"DonHang_{order.Order_code}{ext}");
        }

        // ── Private helpers ───────────────────────────────────────────────

        private async Task<PrintTemplateSettings> BuildSettingsAsync()
        {
            return new PrintTemplateSettings
            {
                CompanyName    = await _settingService.GetAsync(SettingKeys.CompanyName)    ?? "CÔNG TY",
                CompanyAddress = await _settingService.GetAsync(SettingKeys.CompanyAddress) ?? "",
                CompanyPhone   = await _settingService.GetAsync(SettingKeys.CompanyPhone)   ?? "",
                CompanyEmail   = await _settingService.GetAsync(SettingKeys.CompanyEmail)   ?? "",
            };
        }
    }
}
