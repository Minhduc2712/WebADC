using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface IRegionRepository
    {
        Task<Region?> GetByIdAsync(int id);
        Task<Region?> GetByCodeAsync(string regionCode);
        Task<Region?> GetByNameAsync(string regionName);
        Task<IEnumerable<Region>> GetAllAsync();
        Task AddAsync(Region region);
        Task UpdateAsync(Region region);
        Task DeleteAsync(int id);
    }
}
