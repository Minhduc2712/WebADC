using ErpOnlineOrder.Application.DTOs.DistributorDTOs;
using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface IDistributorService
    {
        Task<Distributor?> GetByIdAsync(int id);
        Task<IEnumerable<Distributor>> GetAllAsync();
        Task<Distributor> CreateDistributorAsync(CreateDistributorDto dto, int createdBy);
        Task<bool> UpdateDistributorAsync(UpdateDistributorDto dto, int updatedBy);
        Task<bool> DeleteDistributorAsync(int id);
    }
}