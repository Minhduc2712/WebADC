using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface IAuthorRepository
    {
        Task<Author?> GetByIdAsync(int id);
        Task<Author?> GetByCodeAsync(string code);
        Task<Author?> GetByNameAsync(string name);
        Task<IEnumerable<Author>> GetAllAsync();
        Task<Author> AddAsync(Author author);
        Task UpdateAsync(Author author);
        Task DeleteAsync(int id);
    }
}
