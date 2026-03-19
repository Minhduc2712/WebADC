using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.OrderDTOs;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class OrderApiClient : IOrderApiClient
    {
        private readonly HttpClient _http;

        public OrderApiClient(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("ErpApi");
        }

        public async Task<IEnumerable<OrderDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync("order", cancellationToken);
            if (!response.IsSuccessStatusCode) return Array.Empty<OrderDTO>();
            var list = await response.Content.ReadFromJsonAsync<List<OrderDTO>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<OrderDTO>();
        }

        public async Task<PagedResult<OrderDTO>> GetPagedAsync(int page = 1, int pageSize = 20, string? status = null, string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            var query = new List<string> { $"page={page}", $"pageSize={pageSize}" };
            if (!string.IsNullOrEmpty(status)) query.Add("status=" + Uri.EscapeDataString(status));
            if (!string.IsNullOrEmpty(searchTerm)) query.Add("searchTerm=" + Uri.EscapeDataString(searchTerm));
            var path = "order/paged?" + string.Join("&", query);
            var response = await _http.GetAsync(path, cancellationToken);
            if (!response.IsSuccessStatusCode) return new PagedResult<OrderDTO> { Items = new List<OrderDTO>(), Page = page, PageSize = pageSize, TotalCount = 0 };
            var result = await response.Content.ReadFromJsonAsync<PagedResult<OrderDTO>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return result ?? new PagedResult<OrderDTO>();
        }

        public async Task<OrderDTO?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"order/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<OrderDTO>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<IEnumerable<OrderDTO>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"order/status/{Uri.EscapeDataString(status)}", cancellationToken);
            if (!response.IsSuccessStatusCode) return Array.Empty<OrderDTO>();
            var list = await response.Content.ReadFromJsonAsync<List<OrderDTO>>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return list ?? new List<OrderDTO>();
        }

        public async Task<(bool Success, string? Message, int? Order_id)> CreateOrderAdminAsync(CreateOrderDto model, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync("order/admin", model, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CreateOrderResultDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
                return (result?.Success ?? false, result?.Message, result?.Order_id);
            }
            var msg = await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken);
            return (false, msg, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateOrderAsync(UpdateOrderDto model, CancellationToken cancellationToken = default)
        {
            var response = await _http.PutAsJsonAsync("order", model, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<UpdateOrderResultDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
                return (result?.Success ?? true, null);
            }
            var msg = await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken);
            return (false, msg);
        }

        public async Task<bool> DeleteOrderAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.DeleteAsync($"order/{id}", cancellationToken);
            return response.IsSuccessStatusCode;
        }

        public async Task<int?> CopyOrderAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsync($"order/{id}/copy", null, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            var order = await response.Content.ReadFromJsonAsync<Order>(ErpApiClientHelper.JsonOptions, cancellationToken);
            return order?.Id;
        }

        public async Task<ConfirmOrderResultDto> ConfirmOrderAsync(int id, ConfirmOrderDto model, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync($"order/{id}/confirm", model, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ConfirmOrderResultDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
                return result ?? new ConfirmOrderResultDto { Success = true, Message = "Đã duyệt đơn hàng thành công." };
            }

            var msg = await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken);
            return new ConfirmOrderResultDto
            {
                Success = false,
                Message = string.IsNullOrWhiteSpace(msg) ? "Không thể duyệt đơn hàng." : msg
            };
        }

        public async Task<bool> CancelOrderAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsync($"order/{id}/cancel", null, cancellationToken);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeletePendingOrderAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.DeleteAsync($"order/pending/{id}", cancellationToken);
            return response.IsSuccessStatusCode;
        }

        public async Task<byte[]> ExportOrdersToExcelAsync(CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "order/export");
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
            var response = await _http.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }
    }
}
