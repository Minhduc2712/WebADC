using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<Category?> GetByIdAsync(int id);
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category> CreateCategoryAsync(CreateCategoryDto dto, int createdBy);
        Task<bool> UpdateCategoryAsync(int id, UpdateCategoryDto dto, int updatedBy);
        Task<bool> DeleteCategoryAsync(int id);
    }
}