using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class WarehouseExportApiClient : IWarehouseExportApiClient
    {
        private readonly HttpClient _http;

        public WarehouseExportApiClient(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("ErpApi");
        }

        public async Task<IEnumerable<WarehouseExportDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync("warehouseexport", cancellationToken);
            if (!response.IsSuccessStatusCode) return Array.Empty<WarehouseExportDto>();
            var list = await response.Content.ReadFromJsonAsync<List<WarehouseExportDto>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<WarehouseExportDto>();
        }

        public async Task<PagedResult<WarehouseExportDto>> GetPagedAsync(int page = 1, int pageSize = 20, string? status = null, string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            var query = new List<string> { $"page={page}", $"pageSize={pageSize}" };
            if (!string.IsNullOrEmpty(status)) query.Add("status=" + Uri.EscapeDataString(status));
            if (!string.IsNullOrEmpty(searchTerm)) query.Add("searchTerm=" + Uri.EscapeDataString(searchTerm));
            var path = "warehouseexport/paged?" + string.Join("&", query);
            var response = await _http.GetAsync(path, cancellationToken);
            if (!response.IsSuccessStatusCode) return new PagedResult<WarehouseExportDto> { Items = new List<WarehouseExportDto>(), Page = page, PageSize = pageSize, TotalCount = 0 };
            var result = await response.Content.ReadFromJsonAsync<PagedResult<WarehouseExportDto>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return result ?? new PagedResult<WarehouseExportDto>();
        }

        public async Task<WarehouseExportDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"warehouseexport/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<WarehouseExportDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<IEnumerable<WarehouseExportDto>> GetByInvoiceIdAsync(int invoiceId, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"warehouseexport/invoice/{invoiceId}", cancellationToken);
            if (!response.IsSuccessStatusCode) return Array.Empty<WarehouseExportDto>();
            var list = await response.Content.ReadFromJsonAsync<List<WarehouseExportDto>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<WarehouseExportDto>();
        }

        public async Task<IEnumerable<WarehouseExportDto>> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"warehouseexport/customer/{customerId}", cancellationToken);
            if (!response.IsSuccessStatusCode) return Array.Empty<WarehouseExportDto>();
            var list = await response.Content.ReadFromJsonAsync<List<WarehouseExportDto>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<WarehouseExportDto>();
        }

        public async Task<WarehouseExportDto?> CreateAsync(CreateWarehouseExportDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync("warehouseexport", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<WarehouseExportDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateDeliveryStatusAsync(int id, string status, CancellationToken cancellationToken = default)
        {
            var response = await _http.PatchAsync($"warehouseexport/{id}/delivery-status?status={Uri.EscapeDataString(status)}", null, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<(bool Success, string? Error)> ConfirmAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsync($"warehouseexport/{id}/confirm", null, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<(bool Success, string? Error)> CancelAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsync($"warehouseexport/{id}/cancel", null, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.DeleteAsync($"warehouseexport/{id}", cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<SplitExportResultDto?> SplitAsync(SplitWarehouseExportDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync("warehouseexport/split", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<SplitExportResultDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<MergeExportResultDto?> MergeAsync(MergeWarehouseExportsDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync("warehouseexport/merge", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<MergeExportResultDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<bool> UndoSplitAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsync($"warehouseexport/{id}/undo-split", null, cancellationToken);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UndoMergeAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsync($"warehouseexport/{id}/undo-merge", null, cancellationToken);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> HasExportForInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"warehouseexport/check-invoice/{invoiceId}", cancellationToken);
            if (!response.IsSuccessStatusCode) return false;
            var obj = await response.Content.ReadFromJsonAsync<CheckInvoiceResponse>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return obj?.has_export ?? false;
        }

        public async Task<byte[]> ExportToExcelAsync(string? status = null, CancellationToken cancellationToken = default)
        {
            var path = !string.IsNullOrEmpty(status)
                ? "warehouseexport/export?status=" + Uri.EscapeDataString(status)
                : "warehouseexport/export";
            using var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
            var response = await _http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode) return Array.Empty<byte>();
            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }

        private class CheckInvoiceResponse { public bool has_export { get; set; } }
    }
}
