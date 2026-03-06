using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class PublisherRepository : IPublisherRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public PublisherRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        private IQueryable<Publisher> GetBaseQuery()
        {
            return _context.Publishers.AsNoTracking();
        }

        public async Task<Publisher?> GetByIdAsync(int id)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Publisher?> GetByCodeAsync(string code)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(p => p.Publisher_code == code);
        }

        public async Task<bool> ExistsByCodeAsync(string code, int? excludeId = null)
        {
            var query = GetBaseQuery().Where(p => p.Publisher_code == code);
            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<Publisher?> GetByNameAsync(string name)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(p => p.Publisher_name == name);
        }

        public async Task<IEnumerable<Publisher>> GetAllAsync()
        {
            return await GetBaseQuery()
                .OrderBy(p => p.Publisher_name)
                .ToListAsync();
        }

        public async Task<Publisher> AddAsync(Publisher publisher)
        {
            publisher.Created_at = DateTime.Now;
            publisher.Updated_at = DateTime.Now;
            _context.Publishers.Add(publisher);
            await _context.SaveChangesAsync();
            return publisher;
        }

        public async Task UpdateAsync(Publisher publisher)
        {
            publisher.Updated_at = DateTime.Now;
            _context.Publishers.Update(publisher);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var publisher = await _context.Publishers.FindAsync(id);
            if (publisher != null)
            {
                publisher.Is_deleted = true;
                publisher.Updated_at = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}
