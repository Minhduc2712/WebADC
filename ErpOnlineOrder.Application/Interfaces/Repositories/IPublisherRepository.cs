using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface IPublisherRepository
    {
        Task<Publisher?> GetByIdAsync(int id);
        Task<Publisher?> GetByCodeAsync(string code);
        Task<Publisher?> GetByNameAsync(string name);
        Task<IEnumerable<Publisher>> GetAllAsync();
        Task<Publisher> AddAsync(Publisher publisher);
        Task UpdateAsync(Publisher publisher);
        Task DeleteAsync(int id);
    }
}
