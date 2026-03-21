using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.OrderDTOs;

namespace ErpOnlineOrder.WebMVC.Services.Interfaces
{
    public interface IOrderApiClient
    {
        Task<IEnumerable<OrderDTO>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PagedResult<OrderDTO>> GetPagedAsync(int page = 1, int pageSize = 20, string? status = null, string? searchTerm = null, CancellationToken cancellationToken = default);
        Task<OrderDTO?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<OrderDTO>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Message, int? Order_id)> CreateOrderAdminAsync(CreateOrderDto model, CancellationToken cancellationToken = default);
        Task<(bool Success, string? ErrorMessage)> UpdateOrderAsync(UpdateOrderDto model, CancellationToken cancellationToken = default);
        Task<bool> DeleteOrderAsync(int id, CancellationToken cancellationToken = default);
        Task<int?> CopyOrderAsync(int id, CancellationToken cancellationToken = default);
        Task<ConfirmOrderResultDto> ConfirmOrderAsync(int id, ConfirmOrderDto model, CancellationToken cancellationToken = default);
        Task<(bool Success, string? ErrorMessage)> CustomerApproveOrderAsync(int id, CancellationToken cancellationToken = default);
        Task<(bool Success, string? ErrorMessage)> CustomerRejectOrderAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> CancelOrderAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> DeletePendingOrderAsync(int id, CancellationToken cancellationToken = default);
        Task<byte[]> ExportOrdersToExcelAsync(CancellationToken cancellationToken = default);
        Task<PagedResult<OrderDTO>> GetOrdersByCustomerPagedAsync(int customerId, OrderFilterRequest request, CancellationToken cancellationToken = default);
        Task<IEnumerable<OrderDTO>> GetOrdersByCustomerAsync(int customerId, CancellationToken cancellationToken = default);
        Task<IEnumerable<OrderDTO>> GetMyOrdersAsync(CancellationToken cancellationToken = default);
        Task<CreateOrderResultDto> CreateOrderCustomerAsync(CreateOrderDto model, CancellationToken cancellationToken = default);
    }
}
