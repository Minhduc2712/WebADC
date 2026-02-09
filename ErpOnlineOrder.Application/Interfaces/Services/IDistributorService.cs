using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface IDistributorService
    {
        Task<Distributor?> GetByIdAsync(int id);
        Task<IEnumerable<Distributor>> GetAllAsync();
        Task<Distributor> CreateDistributorAsync(Distributor distributor);
        Task<bool> UpdateDistributorAsync(Distributor distributor);
        Task<bool> DeleteDistributorAsync(int id);
    }
}