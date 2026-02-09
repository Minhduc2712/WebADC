using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface IPublisherService
    {
        Task<Publisher?> GetByIdAsync(int id);
        Task<IEnumerable<Publisher>> GetAllAsync();
        Task<Publisher> CreateAsync(Publisher publisher);
        Task<bool> UpdateAsync(Publisher publisher);
        Task<bool> DeleteAsync(int id);
    }
}
