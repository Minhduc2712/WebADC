using ErpOnlineOrder.Application.DTOs.PackageDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Mappers;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Services
{
    public class PackageService : IPackageService
    {
        private readonly IPackageRepository _packageRepository;

        public PackageService(IPackageRepository packageRepository)
        {
            _packageRepository = packageRepository;
        }

        public async Task<IEnumerable<PackageDto>> GetAllAsync()
        {
            var packages = await _packageRepository.GetAllAsync();
            return packages.Select(EntityMappers.ToPackageDto);
        }

        public async Task<PackageDto?> GetByIdAsync(int id)
        {
            var package = await _packageRepository.GetByIdAsync(id);
            return package == null ? null : EntityMappers.ToPackageDto(package);
        }

        public async Task<IEnumerable<PackageDto>> GetByOrganizationAsync(int organizationId)
        {
            var packages = await _packageRepository.GetByOrganizationAsync(organizationId);
            return packages.Select(EntityMappers.ToPackageDto);
        }

        public async Task<IEnumerable<PackageDto>> GetByRegionAsync(int regionId)
        {
            var packages = await _packageRepository.GetByRegionAsync(regionId);
            return packages.Select(EntityMappers.ToPackageDto);
        }

        public async Task<IEnumerable<PackageDto>> GetByProvinceAsync(int provinceId)
        {
            var packages = await _packageRepository.GetByProvinceAsync(provinceId);
            return packages.Select(EntityMappers.ToPackageDto);
        }

        public async Task<IEnumerable<PackageDto>> GetByWardAsync(int wardId)
        {
            var packages = await _packageRepository.GetByWardAsync(wardId);
            return packages.Select(EntityMappers.ToPackageDto);
        }

        public async Task<PackageDto> CreateAsync(CreatePackageDto dto, int createdBy)
        {
            if (await _packageRepository.ExistsByCodeAsync(dto.Package_code))
                throw new InvalidOperationException($"Gói với mã '{dto.Package_code}' đã tồn tại.");

            var package = new Package
            {
                Package_code = dto.Package_code.Trim(),
                Package_name = dto.Package_name.Trim(),
                Description = dto.Description?.Trim(),
                Organization_information_id = dto.Organization_information_id,
                Region_id = dto.Region_id,
                Province_id = dto.Province_id,
                Ward_id = dto.Ward_id,
                Is_active = dto.Is_active,
                Created_by = createdBy,
                Updated_by = createdBy,
                Is_deleted = false,
                Package_products = dto.Products.Select(p => new Package_product
                {
                    Product_id = p.Product_id,
                    Custom_price = p.Custom_price,
                    Discount_percent = p.Discount_percent,
                    Max_quantity = p.Max_quantity,
                    Is_active = p.Is_active,
                    Created_by = createdBy,
                    Updated_by = createdBy,
                    Is_deleted = false
                }).ToList()
            };

            await _packageRepository.AddAsync(package);
            return EntityMappers.ToPackageDto(package);
        }

        public async Task<bool> UpdateAsync(int id, UpdatePackageDto dto, int updatedBy)
        {
            var package = await _packageRepository.GetByIdAsync(id);
            if (package == null) return false;

            package.Package_name = dto.Package_name.Trim();
            package.Description = dto.Description?.Trim();
            package.Organization_information_id = dto.Organization_information_id;
            package.Region_id = dto.Region_id;
            package.Province_id = dto.Province_id;
            package.Ward_id = dto.Ward_id;
            package.Is_active = dto.Is_active;
            package.Updated_by = updatedBy;

            await _packageRepository.UpdateAsync(package);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var package = await _packageRepository.GetByIdAsync(id);
            if (package == null) return false;
            await _packageRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> AddProductAsync(int packageId, CreatePackageProductDto dto, int createdBy)
        {
            var existing = await _packageRepository.GetPackageProductAsync(packageId, dto.Product_id);
            if (existing != null) return false;

            var pp = new Package_product
            {
                Package_id = packageId,
                Product_id = dto.Product_id,
                Custom_price = dto.Custom_price,
                Discount_percent = dto.Discount_percent,
                Max_quantity = dto.Max_quantity,
                Is_active = dto.Is_active,
                Created_by = createdBy,
                Updated_by = createdBy,
                Is_deleted = false
            };

            await _packageRepository.AddPackageProductAsync(pp);
            return true;
        }

        public async Task<bool> RemoveProductAsync(int packageId, int productId)
        {
            var pp = await _packageRepository.GetPackageProductAsync(packageId, productId);
            if (pp == null) return false;
            await _packageRepository.RemovePackageProductAsync(pp);
            return true;
        }
    }
}
