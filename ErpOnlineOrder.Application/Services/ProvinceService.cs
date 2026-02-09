using ErpOnlineOrder.Application.DTOs.ProvinceDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Services
{
    public class ProvinceService : IProvinceService
    {
        private readonly IProvinceRepository _provinceRepository;
        private readonly IRegionRepository _regionRepository;

        public ProvinceService(IProvinceRepository provinceRepository, IRegionRepository regionRepository)
        {
            _provinceRepository = provinceRepository;
            _regionRepository = regionRepository;
        }

        public async Task<ProvinceDTO?> GetByIdAsync(int id)
        {
            var province = await _provinceRepository.GetByIdAsync(id);
            return province != null ? MapToDto(province) : null;
        }

        public async Task<IEnumerable<ProvinceDTO>> GetAllAsync()
        {
            var provinces = await _provinceRepository.GetAllAsync();
            return provinces.Select(MapToDto);
        }

        public async Task<IEnumerable<ProvinceDTO>> GetByRegionIdAsync(int regionId)
        {
            var provinces = await _provinceRepository.GetByRegionIdAsync(regionId);
            return provinces.Select(MapToDto);
        }

        public async Task<ProvinceDTO?> CreateProvinceAsync(CreateProvinceDto dto, int createdBy)
        {
            // Kiểm tra mã tỉnh thành đã tồn tại
            var existingByCode = await _provinceRepository.GetByCodeAsync(dto.Province_code);
            if (existingByCode != null)
            {
                throw new InvalidOperationException($"Tỉnh thành với mã '{dto.Province_code}' đã tồn tại.");
            }

            // Kiểm tra tên tỉnh thành đã tồn tại
            var existingByName = await _provinceRepository.GetByNameAsync(dto.Province_name);
            if (existingByName != null)
            {
                throw new InvalidOperationException($"Tỉnh thành với tên '{dto.Province_name}' đã tồn tại.");
            }

            // Kiểm tra khu vực tồn tại
            var region = await _regionRepository.GetByIdAsync(dto.Region_id);
            if (region == null)
            {
                throw new InvalidOperationException("Khu vực được chọn không tồn tại trong hệ thống.");
            }

            var province = new Province
            {
                Province_code = dto.Province_code,
                Province_name = dto.Province_name,
                Region_id = dto.Region_id,
                Created_by = createdBy,
                Updated_by = createdBy,
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow,
                Is_deleted = false
            };

            await _provinceRepository.AddAsync(province);
            
            // Load lại để lấy thông tin Region
            var created = await _provinceRepository.GetByIdAsync(province.Id);
            return created != null ? MapToDto(created) : null;
        }

        public async Task<bool> UpdateProvinceAsync(UpdateProvinceDto dto, int updatedBy)
        {
            var province = await _provinceRepository.GetByIdAsync(dto.Id);
            if (province == null)
            {
                return false;
            }

            // Kiểm tra mã tỉnh thành đã tồn tại (nếu thay đổi)
            if (province.Province_code != dto.Province_code)
            {
                var existingByCode = await _provinceRepository.GetByCodeAsync(dto.Province_code);
                if (existingByCode != null)
                {
                    throw new InvalidOperationException($"Tỉnh thành với mã '{dto.Province_code}' đã tồn tại.");
                }
            }

            // Kiểm tra tên tỉnh thành đã tồn tại (nếu thay đổi)
            if (province.Province_name != dto.Province_name)
            {
                var existingByName = await _provinceRepository.GetByNameAsync(dto.Province_name);
                if (existingByName != null)
                {
                    throw new InvalidOperationException($"Tỉnh thành với tên '{dto.Province_name}' đã tồn tại.");
                }
            }

            // Kiểm tra khu vực tồn tại
            var region = await _regionRepository.GetByIdAsync(dto.Region_id);
            if (region == null)
            {
                throw new InvalidOperationException("Khu vực được chọn không tồn tại trong hệ thống.");
            }

            province.Province_code = dto.Province_code;
            province.Province_name = dto.Province_name;
            province.Region_id = dto.Region_id;
            province.Updated_by = updatedBy;
            province.Updated_at = DateTime.UtcNow;

            await _provinceRepository.UpdateAsync(province);
            return true;
        }

        public async Task<bool> DeleteProvinceAsync(int id)
        {
            var province = await _provinceRepository.GetByIdAsync(id);
            if (province == null)
            {
                return false;
            }

            await _provinceRepository.DeleteAsync(id);
            return true;
        }

        private static ProvinceDTO MapToDto(Province province)
        {
            return new ProvinceDTO
            {
                Id = province.Id,
                Province_code = province.Province_code,
                Province_name = province.Province_name,
                Region_id = province.Region_id,
                Region_name = province.Region?.Region_name ?? string.Empty,
                Created_at = province.Created_at,
                Updated_at = province.Updated_at
            };
        }
    }
}
