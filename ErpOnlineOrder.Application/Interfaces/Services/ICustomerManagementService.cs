using ErpOnlineOrder.Application.DTOs.CustomerManagementDTOs;
using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface ICustomerManagementService
    {
        Task<Customer_management?> GetByIdAsync(int id);
        Task<IEnumerable<Customer_management>> GetAllAsync();
        Task<IEnumerable<Customer_management>> GetByStaffAsync(int staffId);
        Task<IEnumerable<Customer_management>> GetByCustomerAsync(int customerId);
        Task<IEnumerable<Customer_management>> GetByProvinceAsync(int provinceId);
        Task<Customer_management> CreateCustomerManagementAsync(CreateCustomerManagementDto dto, int createdBy);
        Task<bool> UpdateCustomerManagementAsync(int id, UpdateCustomerManagementDto dto, int updatedBy);
        Task<bool> DeleteCustomerManagementAsync(int id);
        Task<Customer_management> AssignStaffToCustomerAsync(int staffId, int customerId, int provinceId, int? wardId, int createdBy);
        Task<bool> RemoveAssignmentAsync(int id);
        Task<bool> IsAlreadyAssignedAsync(int staffId, int customerId);
        /// <summary>
        /// Tìm cán bộ quản lý thích hợp theo tỉnh + phường (dùng khi tự động gán cán bộ lúc đăng kí).
        /// </summary>
        Task<Customer_management?> FindAssignmentByProvinceAndWardAsync(int provinceId, int? wardId);
    }
}