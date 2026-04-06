namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface IPrintService
    {
        Task<(byte[] Data, string ContentType, string FileName)> PrintInvoiceAsync(
            int id, int userId, string format, string templateName);

        Task<(byte[] Data, string ContentType, string FileName)> PrintWarehouseExportAsync(
            int id, int userId, string format, string templateName);

        Task<(byte[] Data, string ContentType, string FileName)> PrintOrderAsync(
            int id, int userId, string format, string templateName);

        IEnumerable<(string Name, string DisplayName)> GetInvoiceTemplates();
        IEnumerable<(string Name, string DisplayName)> GetExportTemplates();
        IEnumerable<(string Name, string DisplayName)> GetOrderTemplates();
    }
}
