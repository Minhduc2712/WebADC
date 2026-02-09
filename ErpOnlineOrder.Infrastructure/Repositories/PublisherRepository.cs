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

        public async Task<Publisher?> GetByIdAsync(int id)
        {
            return await _context.Publishers
                .FirstOrDefaultAsync(p => p.Id == id && !p.Is_deleted);
        }

        public async Task<Publisher?> GetByCodeAsync(string code)
        {
            return await _context.Publishers
                .FirstOrDefaultAsync(p => p.Publisher_code == code && !p.Is_deleted);
        }

        public async Task<Publisher?> GetByNameAsync(string name)
        {
            return await _context.Publishers
                .FirstOrDefaultAsync(p => p.Publisher_name == name && !p.Is_deleted);
        }

        public async Task<IEnumerable<Publisher>> GetAllAsync()
        {
            return await _context.Publishers
                .Where(p => !p.Is_deleted)
                .OrderBy(p => p.Publisher_name)
                .ToListAsync();
        }

        public async Task<Publisher> AddAsync(Publisher publisher)
        {
            _context.Publishers.Add(publisher);
            await _context.SaveChangesAsync();
            return publisher;
        }

        public async Task UpdateAsync(Publisher publisher)
        {
            _context.Publishers.Update(publisher);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var publisher = await _context.Publishers.FindAsync(id);
            if (publisher != null)
            {
                publisher.Is_deleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
