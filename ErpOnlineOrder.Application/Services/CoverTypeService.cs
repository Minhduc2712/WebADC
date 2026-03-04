using ErpOnlineOrder.Application.DTOs.CoverTypeDTOs;
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

        public async Task<Cover_type> CreateAsync(CreateCoverTypeDto dto, int createdBy)
        {
            var existingByCode = await _coverTypeRepository.GetByCodeAsync(dto.Cover_type_code);
            if (existingByCode != null)
                throw new InvalidOperationException($"Loại bìa với mã '{dto.Cover_type_code}' đã tồn tại.");

            var existingByName = await _coverTypeRepository.GetByNameAsync(dto.Cover_type_name);
            if (existingByName != null)
                throw new InvalidOperationException($"Loại bìa với tên '{dto.Cover_type_name}' đã tồn tại.");

            var coverType = new Cover_type
            {
                Cover_type_code = dto.Cover_type_code,
                Cover_type_name = dto.Cover_type_name,
                Created_by = createdBy,
                Updated_by = createdBy,
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow
            };
            return await _coverTypeRepository.AddAsync(coverType);
        }

        public async Task<bool> UpdateAsync(int id, UpdateCoverTypeDto dto, int updatedBy)
        {
            var existing = await _coverTypeRepository.GetByIdAsync(id);
            if (existing == null) return false;

            var existingByCode = await _coverTypeRepository.GetByCodeAsync(dto.Cover_type_code);
            if (existingByCode != null && existingByCode.Id != id)
                throw new InvalidOperationException($"Loại bìa với mã '{dto.Cover_type_code}' đã tồn tại.");

            var existingByName = await _coverTypeRepository.GetByNameAsync(dto.Cover_type_name);
            if (existingByName != null && existingByName.Id != id)
                throw new InvalidOperationException($"Loại bìa với tên '{dto.Cover_type_name}' đã tồn tại.");

            existing.Cover_type_code = dto.Cover_type_code;
            existing.Cover_type_name = dto.Cover_type_name;
            existing.Updated_by = updatedBy;
            existing.Updated_at = DateTime.UtcNow;
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
