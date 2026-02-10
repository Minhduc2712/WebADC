using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.OrderDTOs;

namespace ErpOnlineOrder.WebMVC.Services
{
    public interface IOrderApiClient
    {
        Task<IEnumerable<OrderDTO>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<OrderDTO?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<OrderDTO>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Message, int? Order_id)> CreateOrderAdminAsync(CreateOrderDto model, CancellationToken cancellationToken = default);
        Task<bool> UpdateOrderAsync(UpdateOrderDto model, CancellationToken cancellationToken = default);
        Task<bool> DeleteOrderAsync(int id, CancellationToken cancellationToken = default);
        Task<int?> CopyOrderAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ConfirmOrderAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> CancelOrderAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> DeletePendingOrderAsync(int id, CancellationToken cancellationToken = default);
        Task<byte[]> ExportOrdersToExcelAsync(CancellationToken cancellationToken = default);
    }
}
