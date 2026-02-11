using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErpOnlineOrder.Application.Interfaces;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.DTOs.CustomerDTOs;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrganizationRepository _organizationRepository;

        public CustomerService(ICustomerRepository customerRepository, IUserRepository userRepository, IOrganizationRepository organizationRepository)
        {
            _customerRepository = customerRepository;
            _userRepository = userRepository;
            _organizationRepository = organizationRepository;
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _customerRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _customerRepository.GetAllAsync();
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
            var existing = await _customerRepository.GetByIdAsync(id);
            if (existing == null) return false;

            existing.Customer_code = dto.Customer_code;
            existing.Full_name = dto.Full_name;
            existing.Phone_number = dto.Phone_number;
            existing.Address = dto.Address;
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
            existingCustomer.Updated_at = DateTime.UtcNow;
            existingCustomer.Updated_by = userId;
            if (updateCustomerDto.Customer_email != null)
            {
                var user = await _userRepository.GetByIdAsync(userId);
                user.Email = updateCustomerDto.Customer_email;
                await _userRepository.UpdateAsync(user);
            }
            await _customerRepository.UpdateAsync(existingCustomer);
            return true;
        }

        public async Task<bool> UpdateOrganizationAsync(UpdateOrganizationByCustomerDto dto)
        {
            var existingOrganization = await _organizationRepository.GetByCustomerIdAsync(dto.Customer_id);
            if (existingOrganization == null)
            {
                // Nếu chưa có, tạo mới
                var organization = new Organization_information
                {
                    Organization_name = dto.Organization_name,
                    Address = dto.Address,
                    Tax_number = dto.Tax_number,
                    Recipient_name = dto.Recipient_name,
                    Recipient_phone = dto.Recipient_phone,
                    Recipient_address = dto.Recipient_address,
                    Customer_id = dto.Customer_id,
                    Created_by = dto.Customer_id,
                    Created_at = DateTime.UtcNow,
                    Updated_by = dto.Customer_id,
                    Updated_at = DateTime.UtcNow,
                    Is_deleted = false,
                    Organization_code = $"ORG-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"
                };
                await _organizationRepository.AddAsync(organization);
            }
            else
            {
                existingOrganization.Organization_name = dto.Organization_name;
                existingOrganization.Address = dto.Address;
                existingOrganization.Tax_number = dto.Tax_number;
                existingOrganization.Recipient_name = dto.Recipient_name;
                existingOrganization.Recipient_phone = dto.Recipient_phone;
                existingOrganization.Recipient_address = dto.Recipient_address;
                existingOrganization.Updated_by = dto.Customer_id;
                existingOrganization.Updated_at = DateTime.UtcNow;
                await _organizationRepository.UpdateAsync(existingOrganization);
            }
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await _customerRepository.DeleteAsync(id);
            return true;
        }
    }
}
