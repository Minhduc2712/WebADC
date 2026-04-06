using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErpOnlineOrder.Application.Interfaces;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Mappers;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.CustomerDTOs;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly ICustomerManagementRepository _customerManagementRepository;
        private readonly IPermissionService _permissionService;


        public CustomerService(ICustomerRepository customerRepository, IUserRepository userRepository, IStaffRepository staffRepository, ICustomerManagementRepository customerManagementRepository, IPermissionService permissionService)
        {
            _customerRepository = customerRepository;
            _userRepository = userRepository;
            _staffRepository = staffRepository;
            _customerManagementRepository = customerManagementRepository;
            _permissionService = permissionService;
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _customerRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _customerRepository.GetAllAsync();
        }

        public async Task<IEnumerable<CustomerSelectDto>> GetForSelectAsync()
        {
            return await _customerRepository.GetForSelectAsync();
        }

        public async Task<PagedResult<CustomerDTO>> GetAllPagedAsync(CustomerFilterRequest request, int? userId = null)
        {
            IEnumerable<int>? customerIds = null;
            if (userId.HasValue && userId.Value > 0 && !await _permissionService.IsAdminAsync(userId.Value))
            {
                var staff = await _staffRepository.GetByUserIdAsync(userId.Value);
                if (staff != null)
                {
                    customerIds = await _customerManagementRepository.GetCustomerIdsByStaffAsync(staff.Id);
                }
            }

            var paged = await _customerRepository.GetPagedCustomersDTOAsync(request, customerIds);
            var dtos = paged.Items.ToList();
            return new PagedResult<CustomerDTO>
            {
                Items = dtos,
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount
            };
        }

        public async Task<Customer> CreateCustomerAsync(CreateCustomerDto dto, int createdBy)
        {
            var customer = new Customer
            {
                Customer_code = dto.Customer_code,
                Full_name = dto.Full_name,
                Phone_number = dto.Phone_number,
                Address = dto.Address,
                User_id = dto.User_id,
                Province_id = dto.Province_id,
                Ward_id = dto.Ward_id,
                Created_by = createdBy,
                Updated_by = createdBy,
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow,
                Is_deleted = false
            };
            await _customerRepository.AddAsync(customer);
            return customer;
        }

        public async Task<bool> UpdateCustomerByAdminAsync(int id, UpdateCustomerByAdminDto dto, int updatedBy)
        {
            var existing = await _customerRepository.GetByIdBasicAsync(id);
            if (existing == null) return false;

            existing.Customer_code = dto.Customer_code;
            existing.Full_name = dto.Full_name;
            existing.Phone_number = dto.Phone_number;
            existing.Address = dto.Address;
            if (dto.Province_id.HasValue)
            {
                existing.Province_id = dto.Province_id.Value;
                existing.Ward_id = null; // reset ward khi đổi tỉnh
            }
            if (dto.Ward_id.HasValue)
            {
                existing.Ward_id = dto.Ward_id.Value;
            }
            existing.Updated_by = updatedBy;
            existing.Updated_at = DateTime.UtcNow;
            await _customerRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> UpdateCustomerAsync(UpdateCustomerDto updateCustomerDto)
        {
            if (!int.TryParse(updateCustomerDto.User_id, out int userId))
            {
                throw new Exception("Invalid User_id");
            }

            var existingCustomer = await _customerRepository.GetByUserIdAsync(userId);
            if (existingCustomer == null)
            {
                throw new Exception("Không tìm thấy khách hàng");
            }
            existingCustomer.Full_name = updateCustomerDto.Full_name;
            existingCustomer.Phone_number = updateCustomerDto.Phone_number;
            existingCustomer.Address = updateCustomerDto.Address;
            if (updateCustomerDto.Province_id.HasValue)
            {
                existingCustomer.Province_id = updateCustomerDto.Province_id.Value;
                existingCustomer.Ward_id = null;
            }
            if (updateCustomerDto.Ward_id.HasValue)
            {
                existingCustomer.Ward_id = updateCustomerDto.Ward_id.Value;
            }
            existingCustomer.Updated_at = DateTime.UtcNow;
            existingCustomer.Updated_by = userId;
            if (updateCustomerDto.Customer_email != null)
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new Exception("Không tìm thấy tài khoản người dùng");
                user.Email = updateCustomerDto.Customer_email;
                await _userRepository.UpdateAsync(user);
            }
            await _customerRepository.UpdateAsync(existingCustomer);
            return true;
        }

        public async Task<bool> UpdateOrganizationAsync(UpdateOrganizationByCustomerDto dto)
        {
            var customer = await _customerRepository.GetByIdBasicAsync(dto.Customer_id);
            if (customer == null) return false;

            customer.Organization_information_id = dto.Organization_information_id;
            customer.Recipient_name = dto.Recipient_name;
            customer.Recipient_phone = dto.Recipient_phone;
            customer.Recipient_address = dto.Recipient_address;
            customer.Updated_at = DateTime.UtcNow;
            customer.Updated_by = dto.Customer_id;
            await _customerRepository.UpdateAsync(customer);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await _customerRepository.DeleteAsync(id);
            return true;
        }
    }
}
