using ErpOnlineOrder.Application.DTOs.RegionDTOs;
using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface IRegionService
    {
        Task<RegionDTO?> GetByIdAsync(int id);
        Task<IEnumerable<RegionDTO>> GetAllAsync();
        Task<RegionDTO?> CreateRegionAsync(CreateRegionDto dto, int createdBy);
        Task<bool> UpdateRegionAsync(UpdateRegionDto dto, int updatedBy);
        Task<bool> DeleteRegionAsync(int id);
    }
}