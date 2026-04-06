using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.OrderDTOs;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class OrderApiClient : BaseApiClient, IOrderApiClient
    {
        public OrderApiClient(IHttpClientFactory factory) : base(factory.CreateClient("ErpApi"))
        {
        }

        public async Task<IEnumerable<OrderDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<OrderDTO>>("order", cancellationToken) ?? Array.Empty<OrderDTO>();
        }

        public async Task<PagedResult<OrderDTO>> GetPagedAsync(int page = 1, int pageSize = 20, string? status = null, string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            var query = new List<string> { $"page={page}", $"pageSize={pageSize}" };
            if (!string.IsNullOrEmpty(status)) query.Add("status=" + Uri.EscapeDataString(status));
            if (!string.IsNullOrEmpty(searchTerm)) query.Add("searchTerm=" + Uri.EscapeDataString(searchTerm));
            var path = "order/paged?" + string.Join("&", query);
            return await GetAsync<PagedResult<OrderDTO>>(path, cancellationToken) ?? new PagedResult<OrderDTO> { Items = new List<OrderDTO>(), Page = page, PageSize = pageSize, TotalCount = 0 };
        }

        public async Task<OrderDTO?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await GetAsync<OrderDTO>($"order/{id}", cancellationToken);
        }

        public async Task<IEnumerable<OrderDTO>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<OrderDTO>>($"order/status/{Uri.EscapeDataString(status)}", cancellationToken) ?? Array.Empty<OrderDTO>();
        }

        public async Task<(bool Success, string? Message, int? Order_id)> CreateOrderAdminAsync(CreateOrderDto model, CancellationToken cancellationToken = default)
        {
            var (data, err) = await PostAsync<CreateOrderDto, CreateOrderResultDto>("order/admin", model, cancellationToken);
            if (data != null) return (data.Success, data.Message, data.Order_id);
            return (false, err, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateOrderAsync(UpdateOrderDto model, CancellationToken cancellationToken = default)
        {
            var (data, err) = await PutAsync("order", model, cancellationToken);
            return (data, err);
        }

        public async Task<bool> DeleteOrderAsync(int id, CancellationToken cancellationToken = default)
        {
            var (success, _) = await DeleteAsync($"order/{id}", cancellationToken);
            return success;
        }

        public async Task<int?> CopyOrderAsync(int id, CancellationToken cancellationToken = default)
        {
            var (data, _) = await PostAsync<Order>($"order/{id}/copy", cancellationToken);
            return data?.Id;
        }

        public async Task<ConfirmOrderResultDto> ConfirmOrderAsync(int id, ConfirmOrderDto model, CancellationToken cancellationToken = default)
        {
            var (data, err) = await PostAsync<ConfirmOrderDto, ConfirmOrderResultDto>($"order/{id}/confirm", model, cancellationToken);
            if (data != null) return data;
            return new ConfirmOrderResultDto { Success = false, Message = err ?? "Không thể duyệt đơn hàng." };
        }

        public async Task<(bool Success, string? ErrorMessage)> CustomerApproveOrderAsync(int id, CancellationToken cancellationToken = default)
        {
            return await PostWithoutReturnAsync($"order/{id}/customer-approve", cancellationToken);
        }

        public async Task<(bool Success, string? ErrorMessage)> CustomerRejectOrderAsync(int id, CancellationToken cancellationToken = default)
        {
            return await PostWithoutReturnAsync($"order/{id}/customer-reject", cancellationToken);
        }

        public async Task<bool> CancelOrderAsync(int id, CancellationToken cancellationToken = default)
        {
            var (success, _) = await PostWithoutReturnAsync($"order/{id}/cancel", cancellationToken);
            return success;
        }

        public async Task<bool> DeletePendingOrderAsync(int id, CancellationToken cancellationToken = default)
        {
            var (success, _) = await DeleteAsync($"order/pending/{id}", cancellationToken);
            return success;
        }

        public async Task<byte[]> ExportOrdersToExcelAsync(CancellationToken cancellationToken = default)
        {
            return await GetByteArrayAsync("order/export", cancellationToken);
        }

        public async Task<byte[]> DownloadDocumentAsync(int id, string format = "pdf", string template = "standard", CancellationToken cancellationToken = default)
        {
            return await GetByteArrayAsync($"order/{id}/download?format={Uri.EscapeDataString(format)}&template={Uri.EscapeDataString(template)}", cancellationToken);
        }

        public async Task<PagedResult<OrderDTO>> GetOrdersByCustomerPagedAsync(int customerId, OrderFilterRequest request, CancellationToken cancellationToken = default)
        {
            var query = new List<string> { $"page={request.Page}", $"pageSize={request.PageSize}" };
            if (!string.IsNullOrEmpty(request.Status)) query.Add($"status={Uri.EscapeDataString(request.Status)}");
            var path = $"order/customer/{customerId}/paged?" + string.Join("&", query);
            return await GetAsync<PagedResult<OrderDTO>>(path, cancellationToken) ?? new PagedResult<OrderDTO> { Items = new List<OrderDTO>(), Page = request.Page, PageSize = request.PageSize, TotalCount = 0 };
        }

        public async Task<IEnumerable<OrderDTO>> GetOrdersByCustomerAsync(int customerId, CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<OrderDTO>>($"order/customer/{customerId}", cancellationToken) ?? Array.Empty<OrderDTO>();
        }

        public async Task<IEnumerable<OrderDTO>> GetMyOrdersAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<OrderDTO>>("order/my-orders", cancellationToken) ?? Array.Empty<OrderDTO>();
        }

        public async Task<CreateOrderResultDto> CreateOrderCustomerAsync(CreateOrderDto model, CancellationToken cancellationToken = default)
        {
            var (data, err) = await PostAsync<CreateOrderDto, CreateOrderResultDto>("order/customer", model, cancellationToken);
            if (data != null) return data;
            return new CreateOrderResultDto { Success = false, Message = err ?? "Không thể đặt hàng." };
        }
    }
}
