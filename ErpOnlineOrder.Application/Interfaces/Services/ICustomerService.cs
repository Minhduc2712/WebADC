using ErpOnlineOrder.Application.DTOs.CustomerDTOs;
using ErpOnlineOrder.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface ICustomerService
    {
        Task<Customer?> GetByIdAsync(int id);
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<Customer> CreateCustomerAsync(CreateCustomerDto dto, int createdBy);
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateCustomerByAdminAsync(int id, UpdateCustomerByAdminDto dto, int updatedBy);
        Task<bool> UpdateCustomerAsync(UpdateCustomerDto dto);
        Task<bool> UpdateOrganizationAsync(UpdateOrganizationByCustomerDto dto);
    }
}
