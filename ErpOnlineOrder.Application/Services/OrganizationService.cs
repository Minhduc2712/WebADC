using ErpOnlineOrder.Application.DTOs.OrganizationDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Mappers;
using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly ICustomerRepository _customerRepository;

        public OrganizationService(
            IOrganizationRepository organizationRepository,
            ICustomerRepository customerRepository)
        {
            _organizationRepository = organizationRepository;
            _customerRepository = customerRepository;
        }

        public async Task<OrganizationDTO?> GetByIdAsync(int id)
        {
            var org = await _organizationRepository.GetByIdAsync(id);
            return org != null ? EntityMappers.ToOrganizationDto(org) : null;
        }

        public async Task<IEnumerable<OrganizationDTO>> GetAllAsync()
        {
            var orgs = await _organizationRepository.GetAllAsync();
            return orgs.Select(EntityMappers.ToOrganizationDto);
        }

        public async Task<OrganizationDTO?> GetByCustomerIdAsync(int customerId)
        {
            var org = await _organizationRepository.GetByCustomerIdAsync(customerId);
            return org != null ? EntityMappers.ToOrganizationDto(org) : null;
        }

        public async Task<IEnumerable<OrganizationDTO>> SearchAsync(string? keyword)
        {
            var orgs = await _organizationRepository.SearchAsync(keyword);
            return orgs.Select(EntityMappers.ToOrganizationDto);
        }

        public async Task<OrganizationDTO?> CreateOrganizationAsync(CreateOrganizationDto dto, int createdBy)
        {
            // Ki?m tra mã t? ch?c ?ã t?n t?i
            var codeExists = await _organizationRepository.ExistsByCodeAsync(dto.Organization_code);
            if (codeExists)
            {
                throw new Exception("Mã t? ch?c ?ã t?n t?i");
            }

            // Ki?m tra khách hàng t?n t?i
            var customer = await _customerRepository.GetByIdBasicAsync(dto.Customer_id);
            if (customer == null)
            {
                throw new Exception("Khách hàng không t?n t?i");
            }

            var org = new Organization_information
            {
                Organization_code = dto.Organization_code,
                Organization_name = dto.Organization_name,
                Address = dto.Address,
                Tax_number = dto.Tax_number,
                Recipient_name = dto.Recipient_name,
                Recipient_phone = dto.Recipient_phone,
                Recipient_address = dto.Recipient_address,
                Customer_id = dto.Customer_id,
                Created_by = createdBy,
                Updated_by = createdBy,
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow,
                Is_deleted = false
            };

            await _organizationRepository.AddAsync(org);

            // Load l?i ?? l?y thông tin Customer
            var created = await _organizationRepository.GetByIdAsync(org.Id);
            return created != null ? EntityMappers.ToOrganizationDto(created) : null;
        }

        public async Task<bool> UpdateOrganizationAsync(UpdateOrganizationDto dto, int updatedBy)
        {
            var org = await _organizationRepository.GetByIdAsync(dto.Id);
            if (org == null)
            {
                return false;
            }

            // Ki?m tra mã t? ch?c ?ã t?n t?i (n?u thay ??i)
            if (org.Organization_code != dto.Organization_code)
            {
                var codeExists = await _organizationRepository.ExistsByCodeAsync(dto.Organization_code);
                if (codeExists)
                {
                    throw new Exception("Mã t? ch?c ?ã t?n t?i");
                }
            }

            org.Organization_code = dto.Organization_code;
            org.Organization_name = dto.Organization_name;
            org.Address = dto.Address;
            org.Tax_number = dto.Tax_number;
            org.Recipient_name = dto.Recipient_name;
            org.Recipient_phone = dto.Recipient_phone;
            org.Recipient_address = dto.Recipient_address;
            org.Updated_by = updatedBy;
            org.Updated_at = DateTime.UtcNow;

            await _organizationRepository.UpdateAsync(org);
            return true;
        }

        public async Task<bool> DeleteOrganizationAsync(int id)
        {
            var org = await _organizationRepository.GetByIdAsync(id);
            if (org == null)
            {
                return false;
            }

            await _organizationRepository.DeleteAsync(id);
            return true;
        }

    }
}