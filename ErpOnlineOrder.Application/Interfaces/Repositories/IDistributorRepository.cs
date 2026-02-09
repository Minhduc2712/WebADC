using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface IDistributorRepository
    {
        Task<Distributor?> GetByIdAsync(int id);
        Task<Distributor?> GetByCodeAsync(string code);
        Task<Distributor?> GetByNameAsync(string name);
        Task<IEnumerable<Distributor>> GetAllAsync();
        Task AddAsync(Distributor distributor);
        Task UpdateAsync(Distributor distributor);
        Task DeleteAsync(int id);
    }
}
