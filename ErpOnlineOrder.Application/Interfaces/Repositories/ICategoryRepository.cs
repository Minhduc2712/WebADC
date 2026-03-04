using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(int id);
        Task<Category?> GetByCodeAsync(string code);
        Task<Category?> GetByNameAsync(string name);
        Task<IEnumerable<Category>> GetAllAsync();
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
    }
}
