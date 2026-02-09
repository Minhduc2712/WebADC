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

        public async Task<Customer_management> CreateCustomerManagementAsync(Customer_management customerManagement)
        {
            customerManagement.Created_at = DateTime.Now;
            customerManagement.Updated_at = DateTime.Now;
            customerManagement.Is_deleted = false;
            await _customerManagementRepository.AddAsync(customerManagement);
            return customerManagement;
        }

        public async Task<bool> UpdateCustomerManagementAsync(Customer_management customerManagement)
        {
            var existing = await _customerManagementRepository.GetByIdAsync(customerManagement.Id);
            if (existing == null) return false;

            existing.Staff_id = customerManagement.Staff_id;
            existing.Customer_id = customerManagement.Customer_id;
            existing.Province_id = customerManagement.Province_id;
            existing.Updated_by = customerManagement.Updated_by;
            existing.Updated_at = DateTime.Now;
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
                Created_at = DateTime.Now,
                Updated_at = DateTime.Now,
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