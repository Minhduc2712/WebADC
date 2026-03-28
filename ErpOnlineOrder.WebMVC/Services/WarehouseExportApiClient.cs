using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class WarehouseExportApiClient : BaseApiClient, IWarehouseExportApiClient
    {
        public WarehouseExportApiClient(IHttpClientFactory factory) : base(factory.CreateClient("ErpApi"))
        {
        }

        public async Task<IEnumerable<WarehouseExportDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<WarehouseExportDto>>("warehouseexport", cancellationToken) ?? Array.Empty<WarehouseExportDto>();
        }

        public async Task<IEnumerable<WarehouseExportDto>> GetMergeableAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<WarehouseExportDto>>("warehouseexport/mergeable", cancellationToken) ?? Array.Empty<WarehouseExportDto>();
        }

        public async Task<PagedResult<WarehouseExportDto>> GetPagedAsync(int page = 1, int pageSize = 20, string? status = null, string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            var query = new List<string> { $"page={page}", $"pageSize={pageSize}" };
            if (!string.IsNullOrEmpty(status)) query.Add("status=" + Uri.EscapeDataString(status));
            if (!string.IsNullOrEmpty(searchTerm)) query.Add("searchTerm=" + Uri.EscapeDataString(searchTerm));
            var path = "warehouseexport/paged?" + string.Join("&", query);
            return await GetAsync<PagedResult<WarehouseExportDto>>(path, cancellationToken) ?? new PagedResult<WarehouseExportDto> { Items = new List<WarehouseExportDto>(), Page = page, PageSize = pageSize, TotalCount = 0 };
        }

        public async Task<WarehouseExportDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await GetAsync<WarehouseExportDto>($"warehouseexport/{id}", cancellationToken);
        }

        public async Task<IEnumerable<WarehouseExportDto>> GetByInvoiceIdAsync(int invoiceId, CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<WarehouseExportDto>>($"warehouseexport/invoice/{invoiceId}", cancellationToken) ?? Array.Empty<WarehouseExportDto>();
        }

        public async Task<IEnumerable<WarehouseExportDto>> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<WarehouseExportDto>>($"warehouseexport/customer/{customerId}", cancellationToken) ?? Array.Empty<WarehouseExportDto>();
        }

        public async Task<IEnumerable<WarehouseExportDto>> GetMyExportsAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<WarehouseExportDto>>("warehouseexport/my-exports", cancellationToken) ?? Array.Empty<WarehouseExportDto>();
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateWarehouseExportDto dto, CancellationToken cancellationToken = default)
        {
            return await PutAsync($"warehouseexport/{id}", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateDeliveryStatusAsync(int id, string status, CancellationToken cancellationToken = default)
        {
            return await PatchAsync($"warehouseexport/{id}/delivery-status?status={Uri.EscapeDataString(status)}", cancellationToken);
        }

        public async Task<(bool Success, string? Error)> ConfirmAsync(int id, CancellationToken cancellationToken = default)
        {
            return await PostWithoutReturnAsync($"warehouseexport/{id}/confirm", cancellationToken);
        }

        public async Task<(bool Success, string? Error)> CancelAsync(int id, CancellationToken cancellationToken = default)
        {
            return await PostWithoutReturnAsync($"warehouseexport/{id}/cancel", cancellationToken);
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync($"warehouseexport/{id}", cancellationToken);
        }

        public async Task<SplitExportResultDto?> SplitAsync(SplitWarehouseExportDto dto, CancellationToken cancellationToken = default)
        {
            var (data, _) = await PostAsync<SplitWarehouseExportDto, SplitExportResultDto>("warehouseexport/split", dto, cancellationToken);
            return data;
        }

        public async Task<MergeExportResultDto?> MergeAsync(MergeWarehouseExportsDto dto, CancellationToken cancellationToken = default)
        {
            var (data, _) = await PostAsync<MergeWarehouseExportsDto, MergeExportResultDto>("warehouseexport/merge", dto, cancellationToken);
            return data;
        }

        public async Task<bool> UndoSplitAsync(int id, CancellationToken cancellationToken = default)
        {
            var (success, _) = await PostWithoutReturnAsync($"warehouseexport/{id}/undo-split", cancellationToken);
            return success;
        }

        public async Task<bool> UndoMergeAsync(int id, CancellationToken cancellationToken = default)
        {
            var (success, _) = await PostWithoutReturnAsync($"warehouseexport/{id}/undo-merge", cancellationToken);
            return success;
        }

        public async Task<bool> HasExportForInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default)
        {
            var obj = await GetAsync<CheckInvoiceResponse>($"warehouseexport/check-invoice/{invoiceId}", cancellationToken);
            return obj?.has_export ?? false;
        }

        public async Task<(bool Success, string? Error)> UpdateStatusAsync(int id, string status, CancellationToken cancellationToken = default)
        {
            return await PatchAsync($"warehouseexport/{id}/status?status={Uri.EscapeDataString(status)}", cancellationToken);
        }

        public async Task<byte[]> ExportToExcelAsync(string? status = null, CancellationToken cancellationToken = default)
        {
            var path = !string.IsNullOrEmpty(status)
                ? "warehouseexport/export?status=" + Uri.EscapeDataString(status)
                : "warehouseexport/export";
            return await GetByteArrayAsync(path, cancellationToken);
        }

        private class CheckInvoiceResponse { public bool has_export { get; set; } }

        public async Task<PagedResult<WarehouseExportDto>> GetByCustomerIdPagedAsync(int customerId, WarehouseExportFilterRequest request, CancellationToken cancellationToken = default)
        {
            var query = new List<string> { $"page={request.Page}", $"pageSize={request.PageSize}" };
            if (!string.IsNullOrEmpty(request.Status)) query.Add($"status={Uri.EscapeDataString(request.Status)}");
            var path = $"warehouseexport/customer/{customerId}/paged?" + string.Join("&", query);
            return await GetAsync<PagedResult<WarehouseExportDto>>(path, cancellationToken) ?? new PagedResult<WarehouseExportDto> { Items = new List<WarehouseExportDto>(), Page = request.Page, PageSize = request.PageSize, TotalCount = 0 };
        }
    }
}
