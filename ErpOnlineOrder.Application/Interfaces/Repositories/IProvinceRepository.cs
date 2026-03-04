using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface IProvinceRepository
    {
        Task<Province?> GetByIdAsync(int id);
        Task<Province?> GetByCodeAsync(string provinceCode);
        Task<Province?> GetByNameAsync(string provinceName);
        Task<IEnumerable<Province>> GetAllAsync();
        Task<IEnumerable<Province>> GetByRegionIdAsync(int regionId);
        Task AddAsync(Province province);
        Task UpdateAsync(Province province);
        Task DeleteAsync(int id);
    }
}
