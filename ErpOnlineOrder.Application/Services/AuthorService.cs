using ErpOnlineOrder.Application.DTOs.AuthorDTOs;
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

        public async Task<Author> CreateAsync(CreateAuthorDto dto, int createdBy)
        {
            var existingByCode = await _authorRepository.GetByCodeAsync(dto.Author_code);
            if (existingByCode != null)
                throw new InvalidOperationException($"Tác giả với mã '{dto.Author_code}' đã tồn tại.");

            var existingByName = await _authorRepository.GetByNameAsync(dto.Author_name);
            if (existingByName != null)
                throw new InvalidOperationException($"Tác giả với tên '{dto.Author_name}' đã tồn tại.");

            var author = new Author
            {
                Author_code = dto.Author_code,
                Author_name = dto.Author_name,
                Pen_name = dto.Pen_name,
                Email_author = dto.Email_author,
                Phone_number = dto.Phone_number,
                birth_date = dto.birth_date,
                death_date = dto.death_date,
                Nationality = dto.Nationality,
                Biography = dto.Biography,
                Created_by = createdBy,
                Updated_by = createdBy,
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow
            };
            return await _authorRepository.AddAsync(author);
        }

        public async Task<bool> UpdateAsync(int id, UpdateAuthorDto dto, int updatedBy)
        {
            var existing = await _authorRepository.GetByIdAsync(id);
            if (existing == null) return false;

            var existingByCode = await _authorRepository.GetByCodeAsync(dto.Author_code);
            if (existingByCode != null && existingByCode.Id != id)
                throw new InvalidOperationException($"Tác giả với mã '{dto.Author_code}' đã tồn tại.");

            var existingByName = await _authorRepository.GetByNameAsync(dto.Author_name);
            if (existingByName != null && existingByName.Id != id)
                throw new InvalidOperationException($"Tác giả với tên '{dto.Author_name}' đã tồn tại.");

            existing.Author_code = dto.Author_code;
            existing.Author_name = dto.Author_name;
            existing.Pen_name = dto.Pen_name;
            existing.Email_author = dto.Email_author;
            existing.Phone_number = dto.Phone_number;
            existing.birth_date = dto.birth_date;
            existing.death_date = dto.death_date;
            existing.Nationality = dto.Nationality;
            existing.Biography = dto.Biography;
            existing.Updated_by = updatedBy;
            existing.Updated_at = DateTime.UtcNow;
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
