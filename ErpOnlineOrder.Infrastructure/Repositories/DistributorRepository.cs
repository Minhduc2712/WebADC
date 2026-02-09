using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class DistributorRepository : IDistributorRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public DistributorRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        public async Task<Distributor?> GetByIdAsync(int id)
        {
            return await _context.Distributors.FindAsync(id);
        }

        public async Task<Distributor?> GetByCodeAsync(string code)
        {
            return await _context.Distributors
                .FirstOrDefaultAsync(d => d.Distributor_code == code && !d.Is_deleted);
        }

        public async Task<Distributor?> GetByNameAsync(string name)
        {
            return await _context.Distributors
                .FirstOrDefaultAsync(d => d.Distributor_name == name && !d.Is_deleted);
        }

        public async Task<IEnumerable<Distributor>> GetAllAsync()
        {
            return await _context.Distributors.Where(d => !d.Is_deleted).ToListAsync();
        }

        public async Task AddAsync(Distributor distributor)
        {
            await _context.Distributors.AddAsync(distributor);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Distributor distributor)
        {
            _context.Distributors.Update(distributor);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var distributor = await _context.Distributors.FindAsync(id);
            if (distributor != null)
            {
                distributor.Is_deleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
