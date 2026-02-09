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

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            // Kiểm tra trùng lặp Category_code
            var existingByCode = await _categoryRepository.GetByCodeAsync(category.Category_code);
            if (existingByCode != null)
            {
                throw new InvalidOperationException($"Danh mục với mã '{category.Category_code}' đã tồn tại.");
            }

            // Kiểm tra trùng lặp Category_name
            var existingByName = await _categoryRepository.GetByNameAsync(category.Category_name);
            if (existingByName != null)
            {
                throw new InvalidOperationException($"Danh mục với tên '{category.Category_name}' đã tồn tại.");
            }

            category.Created_at = DateTime.UtcNow;
            category.Updated_at = DateTime.UtcNow;
            category.Is_deleted = false;
            await _categoryRepository.AddAsync(category);
            return category;
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            var existing = await _categoryRepository.GetByIdAsync(category.Id);
            if (existing == null)
                return false;

            // Kiểm tra trùng lặp khi update (trừ bản ghi hiện tại)
            var existingByCode = await _categoryRepository.GetByCodeAsync(category.Category_code);
            if (existingByCode != null && existingByCode.Id != category.Id)
            {
                throw new InvalidOperationException($"Danh mục với mã '{category.Category_code}' đã tồn tại.");
            }

            var existingByName = await _categoryRepository.GetByNameAsync(category.Category_name);
            if (existingByName != null && existingByName.Id != category.Id)
            {
                throw new InvalidOperationException($"Danh mục với tên '{category.Category_name}' đã tồn tại.");
            }

            existing.Category_code = category.Category_code;
            existing.Category_name = category.Category_name;
            existing.Updated_by = category.Updated_by;
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
