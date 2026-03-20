using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class InvoiceApiClient : BaseApiClient, IInvoiceApiClient
    {
        public InvoiceApiClient(IHttpClientFactory factory) : base(factory.CreateClient("ErpApi"))
        {
        }

        public async Task<IEnumerable<InvoiceDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<InvoiceDto>>("invoice", cancellationToken) ?? Array.Empty<InvoiceDto>();
        }

        public async Task<IEnumerable<InvoiceDto>> GetForMergeAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<InvoiceDto>>("invoice/for-merge", cancellationToken) ?? Array.Empty<InvoiceDto>();
        }

        public async Task<IEnumerable<InvoiceSelectDto>> GetForWarehouseExportAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<InvoiceSelectDto>>("invoice/for-warehouse-export", cancellationToken) ?? Array.Empty<InvoiceSelectDto>();
        }

        public async Task<PagedResult<InvoiceDto>> GetPagedAsync(int page = 1, int pageSize = 20, string? status = null, string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            var query = new List<string> { $"page={page}", $"pageSize={pageSize}" };
            if (!string.IsNullOrEmpty(status)) query.Add("status=" + Uri.EscapeDataString(status));
            if (!string.IsNullOrEmpty(searchTerm)) query.Add("searchTerm=" + Uri.EscapeDataString(searchTerm));
            var path = "invoice/paged?" + string.Join("&", query);
            return await GetAsync<PagedResult<InvoiceDto>>(path, cancellationToken) ?? new PagedResult<InvoiceDto> { Items = new List<InvoiceDto>(), Page = page, PageSize = pageSize, TotalCount = 0 };
        }

        public async Task<InvoiceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await GetAsync<InvoiceDto>($"invoice/{id}", cancellationToken);
        }

        public async Task<SplitInvoiceResultDto?> SplitAsync(SplitInvoiceDto dto, CancellationToken cancellationToken = default)
        {
            var (data, _) = await PostAsync<SplitInvoiceDto, SplitInvoiceResultDto>("invoice/split", dto, cancellationToken);
            return data;
        }

        public async Task<MergeInvoiceResultDto?> MergeAsync(MergeInvoicesDto dto, CancellationToken cancellationToken = default)
        {
            var (data, _) = await PostAsync<MergeInvoicesDto, MergeInvoiceResultDto>("invoice/merge", dto, cancellationToken);
            return data;
        }

        public async Task<bool> UndoSplitAsync(int parentInvoiceId, CancellationToken cancellationToken = default)
        {
            var (success, _) = await PostWithoutReturnAsync($"invoice/{parentInvoiceId}/undo-split", cancellationToken);
            return success;
        }

        public async Task<bool> UndoMergeAsync(int mergedInvoiceId, CancellationToken cancellationToken = default)
        {
            var (success, _) = await PostWithoutReturnAsync($"invoice/{mergedInvoiceId}/undo-merge", cancellationToken);
            return success;
        }

        // public async Task<CreateInvoiceFromOrderResultDto?> CreateFromOrderAsync(int orderId, CancellationToken cancellationToken = default)
        // {
        //     var response = await _http.PostAsync($"invoice/create-from-order/{orderId}", null, cancellationToken);
        //     if (!response.IsSuccessStatusCode) return null;
        //     return await response.Content.ReadFromJsonAsync<CreateInvoiceFromOrderResultDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        // }

        public async Task<(bool Success, string? Error)> UpdateStatusAsync(int id, string status, CancellationToken cancellationToken = default)
        {
            return await PatchAsync($"invoice/{id}/status?status={Uri.EscapeDataString(status)}", cancellationToken);
        }

        public async Task<byte[]> ExportToExcelAsync(string? status = null, CancellationToken cancellationToken = default)
        {
            var path = !string.IsNullOrEmpty(status)
                ? "invoice/export?status=" + Uri.EscapeDataString(status)
                : "invoice/export";
            return await GetByteArrayAsync(path, cancellationToken);
        }

        public async Task<PagedResult<InvoiceDto>> GetByCustomerIdPagedAsync(int customerId, InvoiceFilterRequest request, CancellationToken cancellationToken = default)
        {
            var query = new List<string> { $"page={request.Page}", $"pageSize={request.PageSize}" };
            if (!string.IsNullOrEmpty(request.Status)) query.Add($"status={Uri.EscapeDataString(request.Status)}");
            var path = $"invoice/customer/{customerId}/paged?" + string.Join("&", query);
            return await GetAsync<PagedResult<InvoiceDto>>(path, cancellationToken) ?? new PagedResult<InvoiceDto> { Items = new List<InvoiceDto>(), Page = request.Page, PageSize = request.PageSize, TotalCount = 0 };
        }
    }
}
