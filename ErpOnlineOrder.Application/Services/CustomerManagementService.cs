using ErpOnlineOrder.Application.DTOs.CustomerManagementDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Services
{
    public class CustomerManagementService : ICustomerManagementService
    {
        private readonly ICustomerManagementRepository _customerManagementRepository;

        public CustomerManagementService(ICustomerManagementRepository customerManagementRepository)
        {
            _customerManagementRepository = customerManagementRepository;
        }

        public async Task<Customer_management?> GetByIdAsync(int id)
        {
            return await _customerManagementRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Customer_management>> GetAllAsync()
        {
            return await _customerManagementRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Customer_management>> GetByStaffAsync(int staffId)
        {
            return await _customerManagementRepository.GetByStaffAsync(staffId);
        }

        public async Task<IEnumerable<Customer_management>> GetByCustomerAsync(int customerId)
        {
            return await _customerManagementRepository.GetByCustomerAsync(customerId);
        }

        public async Task<IEnumerable<Customer_management>> GetByProvinceAsync(int provinceId)
        {
            return await _customerManagementRepository.GetByProvinceAsync(provinceId);
        }

        public async Task<Customer_management> CreateCustomerManagementAsync(CreateCustomerManagementDto dto, int createdBy)
        {
            var existing = await _customerManagementRepository.GetByStaffAndCustomerAsync(dto.Staff_id, dto.Customer_id);
            if (existing != null)
                throw new InvalidOperationException("Cán bộ này đã được gán phụ trách khách hàng này rồi.");

            var assignment = new Customer_management
            {
                Staff_id = dto.Staff_id,
                Customer_id = dto.Customer_id,
                Province_id = dto.Province_id,
                Created_by = createdBy,
                Updated_by = createdBy,
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow,
                Is_deleted = false
            };
            await _customerManagementRepository.AddAsync(assignment);
            return assignment;
        }

        public async Task<bool> UpdateCustomerManagementAsync(int id, UpdateCustomerManagementDto dto, int updatedBy)
        {
            var existing = await _customerManagementRepository.GetByIdAsync(id);
            if (existing == null) return false;

            var duplicate = await _customerManagementRepository.GetByStaffAndCustomerAsync(dto.Staff_id, dto.Customer_id);
            if (duplicate != null && duplicate.Id != id)
                throw new InvalidOperationException("Cán bộ này đã được gán phụ trách khách hàng này rồi.");

            existing.Staff_id = dto.Staff_id;
            existing.Customer_id = dto.Customer_id;
            existing.Province_id = dto.Province_id;
            existing.Updated_by = updatedBy;
            existing.Updated_at = DateTime.UtcNow;
            await _customerManagementRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteCustomerManagementAsync(int id)
        {
            await _customerManagementRepository.DeleteAsync(id);
            return true;
        }

        public async Task<Customer_management> AssignStaffToCustomerAsync(int staffId, int customerId, int provinceId, int createdBy)
        {
            // Kiểm tra đã gán chưa
            var existing = await _customerManagementRepository.GetByStaffAndCustomerAsync(staffId, customerId);
            if (existing != null)
            {
                throw new InvalidOperationException("Cán bộ này đã được gán phụ trách khách hàng này rồi.");
            }

            var assignment = new Customer_management
            {
                Staff_id = staffId,
                Customer_id = customerId,
                Province_id = provinceId,
                Created_by = createdBy,
                Updated_by = createdBy,
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow,
                Is_deleted = false
            };

            await _customerManagementRepository.AddAsync(assignment);
            return assignment;
        }

        public async Task<bool> RemoveAssignmentAsync(int id)
        {
            await _customerManagementRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> IsAlreadyAssignedAsync(int staffId, int customerId)
        {
            var existing = await _customerManagementRepository.GetByStaffAndCustomerAsync(staffId, customerId);
            return existing != null;
        }
    }
}