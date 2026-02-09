using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.OrderDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface IOrderService
    {
        Task<OrderDTO?> GetByIdAsync(int id);
        Task<IEnumerable<OrderDTO>> GetAllAsync();
        Task<Order?> GetByCodeAsync(string code);
        Task<CreateOrderResultDto> CreateOrderAsync(CreateOrderDto dto);
        Task<CreateOrderResultDto> CreateOrderWithoutValidationAsync(CreateOrderDto dto);
        
        Task<bool> UpdateOrderAsync(UpdateOrderDto dto);
        Task<bool> DeleteOrderAsync(int id);
        Task<Order> CopyOrderAsync(int id);
        Task<IEnumerable<OrderDTO>> GetOrdersByStaffAsync(int staffId);
        Task<IEnumerable<OrderDTO>> GetOrdersByCustomerAsync(int customerId);
        Task<bool> ConfirmOrderAsync(int id);
        Task<bool> CancelOrderAsync(int id);
        Task<byte[]> ExportOrdersToExcelAsync();
        Task<IEnumerable<OrderDTO>> GetOrdersByStatusAsync(string status);
        Task<bool> DeletePendingOrderAsync(int id);
        Task<IEnumerable<CustomerAllowedProductDto>> GetAllowedProductsForCustomerAsync(int customerId);
        Task<bool> CanCustomerOrderProductAsync(int customerId, int productId);
        Task<decimal> GetProductPriceForCustomerAsync(int customerId, int productId);
    }
}