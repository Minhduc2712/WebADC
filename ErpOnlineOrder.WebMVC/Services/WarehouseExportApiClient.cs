using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs;

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

        private class CheckInvoiceResponse { public bool has_export { get; set; } }
    }
}
