using ErpOnlineOrder.Application.DTOs.ProvinceDTOs;
using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface IProvinceService
    {
        Task<ProvinceDTO?> GetByIdAsync(int id);
        Task<IEnumerable<ProvinceDTO>> GetAllAsync();
        Task<IEnumerable<ProvinceDTO>> GetByRegionIdAsync(int regionId);
        Task<ProvinceDTO?> CreateProvinceAsync(CreateProvinceDto dto, int createdBy);
        Task<bool> UpdateProvinceAsync(UpdateProvinceDto dto, int updatedBy);
        Task<bool> DeleteProvinceAsync(int id);
    }
}