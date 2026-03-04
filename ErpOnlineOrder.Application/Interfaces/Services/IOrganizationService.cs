using ErpOnlineOrder.Application.DTOs.OrganizationDTOs;
using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface IOrganizationService
    {
        Task<OrganizationDTO?> GetByIdAsync(int id);
        Task<IEnumerable<OrganizationDTO>> GetAllAsync();
        Task<OrganizationDTO?> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<OrganizationDTO>> SearchAsync(string? keyword);
        Task<OrganizationDTO?> CreateOrganizationAsync(CreateOrganizationDto dto, int createdBy);
        Task<bool> UpdateOrganizationAsync(UpdateOrganizationDto dto, int updatedBy);
        Task<bool> DeleteOrganizationAsync(int id);
    }
}