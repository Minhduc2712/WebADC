using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class AuthorRepository : IAuthorRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public AuthorRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        private IQueryable<Author> GetBaseQuery()
        {
            return _context.Authors.AsNoTracking();
        }

        public async Task<Author?> GetByIdAsync(int id)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Author?> GetByCodeAsync(string code)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(a => a.Author_code == code);
        }

        public async Task<bool> ExistsByCodeAsync(string code, int? excludeId = null)
        {
            var query = GetBaseQuery().Where(a => a.Author_code == code);
            if (excludeId.HasValue)
                query = query.Where(a => a.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<Author?> GetByNameAsync(string name)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(a => a.Author_name == name);
        }

        public async Task<IEnumerable<Author>> GetAllAsync()
        {
            return await GetBaseQuery()
                .OrderBy(a => a.Author_name)
                .ToListAsync();
        }

        public async Task<Author> AddAsync(Author author)
        {
            author.Created_at = DateTime.Now;
            author.Updated_at = DateTime.Now;
            _context.Authors.Add(author);
            await _context.SaveChangesAsync();
            return author;
        }

        public async Task UpdateAsync(Author author)
        {
            author.Updated_at = DateTime.Now;
            _context.Authors.Update(author);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author != null)
            {
                author.Is_deleted = true;
                author.Updated_at = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}
