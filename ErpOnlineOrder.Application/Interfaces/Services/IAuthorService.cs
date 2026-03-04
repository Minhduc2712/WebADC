using ErpOnlineOrder.Application.DTOs.AuthorDTOs;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface IAuthorService
    {
        Task<Author?> GetByIdAsync(int id);
        Task<IEnumerable<Author>> GetAllAsync();
        Task<Author> CreateAsync(CreateAuthorDto dto, int createdBy);
        Task<bool> UpdateAsync(int id, UpdateAuthorDto dto, int updatedBy);
        Task<bool> DeleteAsync(int id);
    }
}
