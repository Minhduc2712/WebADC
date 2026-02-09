using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Services
{
    public class AuthorService : IAuthorService
    {
        private readonly IAuthorRepository _authorRepository;

        public AuthorService(IAuthorRepository authorRepository)
        {
            _authorRepository = authorRepository;
        }

        public async Task<Author?> GetByIdAsync(int id)
        {
            return await _authorRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Author>> GetAllAsync()
        {
            return await _authorRepository.GetAllAsync();
        }

        public async Task<Author> CreateAsync(Author author)
        {
            // Kiểm tra trùng lặp Author_code
            var existingByCode = await _authorRepository.GetByCodeAsync(author.Author_code);
            if (existingByCode != null)
            {
                throw new InvalidOperationException($"Tác giả với mã '{author.Author_code}' đã tồn tại.");
            }

            // Kiểm tra trùng lặp Author_name
            var existingByName = await _authorRepository.GetByNameAsync(author.Author_name);
            if (existingByName != null)
            {
                throw new InvalidOperationException($"Tác giả với tên '{author.Author_name}' đã tồn tại.");
            }

            author.Created_at = DateTime.Now;
            author.Updated_at = DateTime.Now;
            return await _authorRepository.AddAsync(author);
        }

        public async Task<bool> UpdateAsync(Author author)
        {
            var existing = await _authorRepository.GetByIdAsync(author.Id);
            if (existing == null) return false;

            // Kiểm tra trùng lặp khi update (trừ bản ghi hiện tại)
            var existingByCode = await _authorRepository.GetByCodeAsync(author.Author_code);
            if (existingByCode != null && existingByCode.Id != author.Id)
            {
                throw new InvalidOperationException($"Tác giả với mã '{author.Author_code}' đã tồn tại.");
            }

            var existingByName = await _authorRepository.GetByNameAsync(author.Author_name);
            if (existingByName != null && existingByName.Id != author.Id)
            {
                throw new InvalidOperationException($"Tác giả với tên '{author.Author_name}' đã tồn tại.");
            }

            existing.Author_code = author.Author_code;
            existing.Author_name = author.Author_name;
            existing.Pen_name = author.Pen_name;
            existing.Email_author = author.Email_author;
            existing.Phone_number = author.Phone_number;
            existing.birth_date = author.birth_date;
            existing.death_date = author.death_date;
            existing.Nationality = author.Nationality;
            existing.Biography = author.Biography;
            existing.Updated_by = author.Updated_by;
            existing.Updated_at = DateTime.Now;
            await _authorRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await _authorRepository.DeleteAsync(id);
            return true;
        }
    }
}
