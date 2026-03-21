using ErpOnlineOrder.Application.DTOs.WardDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Mappers;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Services
{
    public class WardService : IWardService
    {
        private readonly IWardRepository _wardRepository;
        private readonly IProvinceRepository _provinceRepository;

        public WardService(IWardRepository wardRepository, IProvinceRepository provinceRepository)
        {
            _wardRepository = wardRepository;
            _provinceRepository = provinceRepository;
        }

        public async Task<WardDTO?> GetByIdAsync(int id)
        {
            var ward = await _wardRepository.GetByIdAsync(id);
            return ward != null ? EntityMappers.ToWardDto(ward) : null;
        }

        public async Task<IEnumerable<WardDTO>> GetAllAsync()
        {
            var wards = await _wardRepository.GetAllAsync();
            return wards.Select(EntityMappers.ToWardDto);
        }

        public async Task<IEnumerable<WardDTO>> GetByProvinceIdAsync(int provinceId)
        {
            var wards = await _wardRepository.GetByProvinceIdAsync(provinceId);
            return wards.Select(EntityMappers.ToWardDto);
        }

        public async Task<WardDTO?> CreateWardAsync(CreateWardDto dto, int createdBy)
        {
            var codeExists = await _wardRepository.ExistsByCodeAsync(dto.Ward_code);
            if (codeExists)
                throw new InvalidOperationException($"Phường/xã với mã '{dto.Ward_code}' đã tồn tại.");

            var province = await _provinceRepository.GetByIdAsync(dto.Province_id);
            if (province == null)
                throw new InvalidOperationException("Tỉnh/thành phố được chọn không tồn tại.");

            var ward = new Ward
            {
                Ward_code = dto.Ward_code.Trim(),
                Ward_name = dto.Ward_name.Trim(),
                Province_id = dto.Province_id,
                Created_by = createdBy,
                Updated_by = createdBy,
                Is_deleted = false
            };

            await _wardRepository.AddAsync(ward);
            return EntityMappers.ToWardDto(ward);
        }

        public async Task<bool> UpdateWardAsync(int id, UpdateWardDto dto, int updatedBy)
        {
            var existing = await _wardRepository.GetByIdAsync(id);
            if (existing == null) return false;

            var codeExists = await _wardRepository.ExistsByCodeAsync(dto.Ward_code, id);
            if (codeExists)
                throw new InvalidOperationException($"Phường/xã với mã '{dto.Ward_code}' đã tồn tại.");

            var province = await _provinceRepository.GetByIdAsync(dto.Province_id);
            if (province == null)
                throw new InvalidOperationException("Tỉnh/thành phố được chọn không tồn tại.");

            existing.Ward_code = dto.Ward_code.Trim();
            existing.Ward_name = dto.Ward_name.Trim();
            existing.Province_id = dto.Province_id;
            existing.Updated_by = updatedBy;

            await _wardRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteWardAsync(int id)
        {
            await _wardRepository.DeleteAsync(id);
            return true;
        }
    }
}
