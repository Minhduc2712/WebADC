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

        public async Task<Author?> GetByIdAsync(int id)
        {
            return await _context.Authors
                .FirstOrDefaultAsync(a => a.Id == id && !a.Is_deleted);
        }

        public async Task<Author?> GetByCodeAsync(string code)
        {
            return await _context.Authors
                .FirstOrDefaultAsync(a => a.Author_code == code && !a.Is_deleted);
        }

        public async Task<Author?> GetByNameAsync(string name)
        {
            return await _context.Authors
                .FirstOrDefaultAsync(a => a.Author_name == name && !a.Is_deleted);
        }

        public async Task<IEnumerable<Author>> GetAllAsync()
        {
            return await _context.Authors
                .Where(a => !a.Is_deleted)
                .OrderBy(a => a.Author_name)
                .ToListAsync();
        }

        public async Task<Author> AddAsync(Author author)
        {
            _context.Authors.Add(author);
            await _context.SaveChangesAsync();
            return author;
        }

        public async Task UpdateAsync(Author author)
        {
            _context.Authors.Update(author);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author != null)
            {
                author.Is_deleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
