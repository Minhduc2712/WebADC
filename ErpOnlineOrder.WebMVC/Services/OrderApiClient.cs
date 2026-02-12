using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.OrderDTOs;
using ErpOnlineOrder.Domain.Models;

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

        public async Task<bool> ConfirmOrderAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsync($"order/{id}/confirm", null, cancellationToken);
            return response.IsSuccessStatusCode;
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
            var response = await _http.GetAsync("order/export", cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }
    }
}
