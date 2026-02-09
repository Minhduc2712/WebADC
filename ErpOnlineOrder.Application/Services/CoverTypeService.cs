using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Services
{
    public class CoverTypeService : ICoverTypeService
    {
        private readonly ICoverTypeRepository _coverTypeRepository;

        public CoverTypeService(ICoverTypeRepository coverTypeRepository)
        {
            _coverTypeRepository = coverTypeRepository;
        }

        public async Task<Cover_type?> GetByIdAsync(int id)
        {
            return await _coverTypeRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Cover_type>> GetAllAsync()
        {
            return await _coverTypeRepository.GetAllAsync();
        }

        public async Task<Cover_type> CreateAsync(Cover_type coverType)
        {
            // Kiểm tra trùng lặp Cover_type_code
            var existingByCode = await _coverTypeRepository.GetByCodeAsync(coverType.Cover_type_code);
            if (existingByCode != null)
            {
                throw new InvalidOperationException($"Loại bìa với mã '{coverType.Cover_type_code}' đã tồn tại.");
            }

            // Kiểm tra trùng lặp Cover_type_name
            var existingByName = await _coverTypeRepository.GetByNameAsync(coverType.Cover_type_name);
            if (existingByName != null)
            {
                throw new InvalidOperationException($"Loại bìa với tên '{coverType.Cover_type_name}' đã tồn tại.");
            }

            coverType.Created_at = DateTime.Now;
            coverType.Updated_at = DateTime.Now;
            return await _coverTypeRepository.AddAsync(coverType);
        }

        public async Task<bool> UpdateAsync(Cover_type coverType)
        {
            var existing = await _coverTypeRepository.GetByIdAsync(coverType.Id);
            if (existing == null) return false;

            // Kiểm tra trùng lặp khi update (trừ bản ghi hiện tại)
            var existingByCode = await _coverTypeRepository.GetByCodeAsync(coverType.Cover_type_code);
            if (existingByCode != null && existingByCode.Id != coverType.Id)
            {
                throw new InvalidOperationException($"Loại bìa với mã '{coverType.Cover_type_code}' đã tồn tại.");
            }

            var existingByName = await _coverTypeRepository.GetByNameAsync(coverType.Cover_type_name);
            if (existingByName != null && existingByName.Id != coverType.Id)
            {
                throw new InvalidOperationException($"Loại bìa với tên '{coverType.Cover_type_name}' đã tồn tại.");
            }

            existing.Cover_type_code = coverType.Cover_type_code;
            existing.Cover_type_name = coverType.Cover_type_name;
            existing.Updated_by = coverType.Updated_by;
            existing.Updated_at = DateTime.Now;
            await _coverTypeRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await _coverTypeRepository.DeleteAsync(id);
            return true;
        }
    }
}
