using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _categoryRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        public async Task<Category> CreateCategoryAsync(CreateCategoryDto dto, int createdBy)
        {
            var existingByCode = await _categoryRepository.GetByCodeAsync(dto.Category_code);
            if (existingByCode != null)
                throw new InvalidOperationException($"Danh mục với mã '{dto.Category_code}' đã tồn tại.");

            var existingByName = await _categoryRepository.GetByNameAsync(dto.Category_name);
            if (existingByName != null)
                throw new InvalidOperationException($"Danh mục với tên '{dto.Category_name}' đã tồn tại.");

            var category = new Category
            {
                Category_code = dto.Category_code,
                Category_name = dto.Category_name,
                Created_by = createdBy,
                Updated_by = createdBy,
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow,
                Is_deleted = false
            };
            await _categoryRepository.AddAsync(category);
            return category;
        }

        public async Task<bool> UpdateCategoryAsync(int id, UpdateCategoryDto dto, int updatedBy)
        {
            var existing = await _categoryRepository.GetByIdAsync(id);
            if (existing == null) return false;

            var existingByCode = await _categoryRepository.GetByCodeAsync(dto.Category_code);
            if (existingByCode != null && existingByCode.Id != id)
                throw new InvalidOperationException($"Danh mục với mã '{dto.Category_code}' đã tồn tại.");

            var existingByName = await _categoryRepository.GetByNameAsync(dto.Category_name);
            if (existingByName != null && existingByName.Id != id)
                throw new InvalidOperationException($"Danh mục với tên '{dto.Category_name}' đã tồn tại.");

            existing.Category_code = dto.Category_code;
            existing.Category_name = dto.Category_name;
            existing.Updated_by = updatedBy;
            existing.Updated_at = DateTime.UtcNow;
            await _categoryRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            await _categoryRepository.DeleteAsync(id);
            return true;
        }
    }
}
