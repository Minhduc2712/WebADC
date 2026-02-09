using ErpOnlineOrder.Application.DTOs.RegionDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Services
{
    public class RegionService : IRegionService
    {
        private readonly IRegionRepository _regionRepository;

        public RegionService(IRegionRepository regionRepository)
        {
            _regionRepository = regionRepository;
        }

        public async Task<RegionDTO?> GetByIdAsync(int id)
        {
            var region = await _regionRepository.GetByIdAsync(id);
            return region != null ? MapToDto(region) : null;
        }

        public async Task<IEnumerable<RegionDTO>> GetAllAsync()
        {
            var regions = await _regionRepository.GetAllAsync();
            return regions.Select(MapToDto);
        }

        public async Task<RegionDTO?> CreateRegionAsync(CreateRegionDto dto, int createdBy)
        {
            // Kiểm tra mã khu vực đã tồn tại
            var existingByCode = await _regionRepository.GetByCodeAsync(dto.Region_code);
            if (existingByCode != null)
            {
                throw new InvalidOperationException($"Khu vực với mã '{dto.Region_code}' đã tồn tại.");
            }

            // Kiểm tra tên khu vực đã tồn tại
            var existingByName = await _regionRepository.GetByNameAsync(dto.Region_name);
            if (existingByName != null)
            {
                throw new InvalidOperationException($"Khu vực với tên '{dto.Region_name}' đã tồn tại.");
            }

            var region = new Region
            {
                Region_code = dto.Region_code,
                Region_name = dto.Region_name,
                Created_by = createdBy,
                Updated_by = createdBy,
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow,
                Is_deleted = false
            };

            await _regionRepository.AddAsync(region);
            return MapToDto(region);
        }

        public async Task<bool> UpdateRegionAsync(UpdateRegionDto dto, int updatedBy)
        {
            var region = await _regionRepository.GetByIdAsync(dto.Id);
            if (region == null)
            {
                return false;
            }

            // Kiểm tra mã khu vực đã tồn tại (nếu thay đổi)
            if (region.Region_code != dto.Region_code)
            {
                var existingByCode = await _regionRepository.GetByCodeAsync(dto.Region_code);
                if (existingByCode != null)
                {
                    throw new InvalidOperationException($"Khu vực với mã '{dto.Region_code}' đã tồn tại.");
                }
            }

            // Kiểm tra tên khu vực đã tồn tại (nếu thay đổi)
            if (region.Region_name != dto.Region_name)
            {
                var existingByName = await _regionRepository.GetByNameAsync(dto.Region_name);
                if (existingByName != null)
                {
                    throw new InvalidOperationException($"Khu vực với tên '{dto.Region_name}' đã tồn tại.");
                }
            }

            region.Region_code = dto.Region_code;
            region.Region_name = dto.Region_name;
            region.Updated_by = updatedBy;
            region.Updated_at = DateTime.UtcNow;

            await _regionRepository.UpdateAsync(region);
            return true;
        }

        public async Task<bool> DeleteRegionAsync(int id)
        {
            var region = await _regionRepository.GetByIdAsync(id);
            if (region == null)
            {
                return false;
            }

            // Kiểm tra có tỉnh thành nào thuộc khu vực này không
            if (region.Provinces != null && region.Provinces.Any(p => !p.Is_deleted))
            {
                throw new InvalidOperationException($"Không thể xóa khu vực '{region.Region_name}' vì đang có {region.Provinces.Count(p => !p.Is_deleted)} tỉnh thành thuộc khu vực này.");
            }

            await _regionRepository.DeleteAsync(id);
            return true;
        }

        private static RegionDTO MapToDto(Region region)
        {
            return new RegionDTO
            {
                Id = region.Id,
                Region_code = region.Region_code,
                Region_name = region.Region_name,
                Province_count = region.Provinces?.Count(p => !p.Is_deleted) ?? 0,
                Created_at = region.Created_at,
                Updated_at = region.Updated_at
            };
        }
    }
}
