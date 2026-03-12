using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class InvoiceApiClient : IInvoiceApiClient
    {
        private readonly HttpClient _http;

        public InvoiceApiClient(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("ErpApi");
        }

        public async Task<IEnumerable<InvoiceDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync("invoice", cancellationToken);
            if (!response.IsSuccessStatusCode) return Array.Empty<InvoiceDto>();
            var list = await response.Content.ReadFromJsonAsync<List<InvoiceDto>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<InvoiceDto>();
        }

        public async Task<IEnumerable<InvoiceDto>> GetForMergeAsync(CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync("invoice/for-merge", cancellationToken);
            if (!response.IsSuccessStatusCode) return Array.Empty<InvoiceDto>();
            var list = await response.Content.ReadFromJsonAsync<List<InvoiceDto>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<InvoiceDto>();
        }

        public async Task<IEnumerable<InvoiceSelectDto>> GetForWarehouseExportAsync(CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync("invoice/for-warehouse-export", cancellationToken);
            if (!response.IsSuccessStatusCode) return Array.Empty<InvoiceSelectDto>();
            var list = await response.Content.ReadFromJsonAsync<List<InvoiceSelectDto>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<InvoiceSelectDto>();
        }

        public async Task<PagedResult<InvoiceDto>> GetPagedAsync(int page = 1, int pageSize = 20, string? status = null, string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            var query = new List<string> { $"page={page}", $"pageSize={pageSize}" };
            if (!string.IsNullOrEmpty(status)) query.Add("status=" + Uri.EscapeDataString(status));
            if (!string.IsNullOrEmpty(searchTerm)) query.Add("searchTerm=" + Uri.EscapeDataString(searchTerm));
            var path = "invoice/paged?" + string.Join("&", query);
            var response = await _http.GetAsync(path, cancellationToken);
            if (!response.IsSuccessStatusCode) return new PagedResult<InvoiceDto> { Items = new List<InvoiceDto>(), Page = page, PageSize = pageSize, TotalCount = 0 };
            var result = await response.Content.ReadFromJsonAsync<PagedResult<InvoiceDto>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return result ?? new PagedResult<InvoiceDto>();
        }

        public async Task<InvoiceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"invoice/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<InvoiceDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<SplitInvoiceResultDto?> SplitAsync(SplitInvoiceDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync("invoice/split", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<SplitInvoiceResultDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<MergeInvoiceResultDto?> MergeAsync(MergeInvoicesDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync("invoice/merge", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<MergeInvoiceResultDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<bool> UndoSplitAsync(int parentInvoiceId, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsync($"invoice/{parentInvoiceId}/undo-split", null, cancellationToken);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UndoMergeAsync(int mergedInvoiceId, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsync($"invoice/{mergedInvoiceId}/undo-merge", null, cancellationToken);
            return response.IsSuccessStatusCode;
        }

        public async Task<CreateInvoiceFromOrderResultDto?> CreateFromOrderAsync(int orderId, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsync($"invoice/create-from-order/{orderId}", null, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<CreateInvoiceFromOrderResultDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateStatusAsync(int id, string status, CancellationToken cancellationToken = default)
        {
            var response = await _http.PatchAsync($"invoice/{id}/status?status={Uri.EscapeDataString(status)}", null, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<byte[]> ExportToExcelAsync(string? status = null, CancellationToken cancellationToken = default)
        {
            var path = !string.IsNullOrEmpty(status)
                ? "invoice/export?status=" + Uri.EscapeDataString(status)
                : "invoice/export";
            using var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
            var response = await _http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode) return Array.Empty<byte>();
            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }
    }
}
