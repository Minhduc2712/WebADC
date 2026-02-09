using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface IStaffRepository
    {
        Task<Staff?> GetByIdAsync(int id);
        Task<IEnumerable<Staff>> GetAllAsync();
        Task AddAsync(Staff staff);
        Task UpdateAsync(Staff staff);
        Task DeleteAsync(int id);
    }
}