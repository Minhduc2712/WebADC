using ErpOnlineOrder.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface IOrganizationRepository
    {
        Task<Organization_information?> GetByIdAsync(int id);
        Task<Organization_information?> GetByCodeAsync(string organizationCode);
        Task<IEnumerable<Organization_information>> GetAllAsync();
        Task<Organization_information?> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Organization_information>> SearchAsync(string? keyword);
        Task AddAsync(Organization_information organization);
        Task UpdateAsync(Organization_information organization);
        Task DeleteAsync(int id);
    }
}