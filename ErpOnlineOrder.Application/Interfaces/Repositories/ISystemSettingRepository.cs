using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface ISystemSettingRepository
    {
        Task<SystemSetting?> GetByKeyAsync(string key);
        Task<IEnumerable<SystemSetting>> GetAllAsync();
        Task AddAsync(SystemSetting setting);
        Task UpdateAsync(SystemSetting setting);
    }
}
