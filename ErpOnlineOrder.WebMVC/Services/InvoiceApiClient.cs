using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;

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
    }
}
