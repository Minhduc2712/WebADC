using ErpOnlineOrder.Application.DTOs.CustomerPackageDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Services
{
    public class CustomerPackageService : ICustomerPackageService
    {
        private readonly ICustomerPackageRepository _repo;
        private readonly IPermissionService _permissionService;
        private readonly IStaffRepository _staffRepository;
        private readonly ICustomerManagementRepository _customerManagementRepository;

        public CustomerPackageService(
            ICustomerPackageRepository repo,
            IPermissionService permissionService,
            IStaffRepository staffRepository,
            ICustomerManagementRepository customerManagementRepository)
        {
            _repo = repo;
            _permissionService = permissionService;
            _staffRepository = staffRepository;
            _customerManagementRepository = customerManagementRepository;
        }

        public async Task<bool> IsUserAllowedForCustomerAsync(int? userId, int customerId)
        {
            if (!userId.HasValue || userId <= 0) return true;
            if (await _permissionService.IsAdminAsync(userId.Value)) return true;
            var staff = await _staffRepository.GetByUserIdAsync(userId.Value);
            if (staff == null) return false;
            return await _customerManagementRepository.ExistsAsync(staff.Id, customerId);
        }

        public async Task<CustomerPackageDto?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id, includeDetails: true);
            return entity == null ? null : ToDto(entity);
        }

        public async Task<CustomerPackageDto?> GetByCustomerAndPackageAsync(int customerId, int packageId)
        {
            var entity = await _repo.GetByCustomerAndPackageAsync(customerId, packageId, includeDetails: true);
            return entity == null ? null : ToDto(entity);
        }

        public async Task<IEnumerable<CustomerPackageDto>> GetByCustomerIdAsync(int customerId)
        {
            var list = await _repo.GetByCustomerIdAsync(customerId, includeDetails: true);
            return list.Select(ToDto);
        }

        public async Task<IEnumerable<CustomerPackageDto>> GetByPackageIdAsync(int packageId)
        {
            var list = await _repo.GetByPackageIdAsync(packageId, includeDetails: true);
            return list.Select(ToDto);
        }

        public async Task<IEnumerable<int>> GetProductIdsByCustomerIdAsync(int customerId)
        {
            return await _repo.GetProductIdsByCustomerIdAsync(customerId);
        }

        public async Task<CustomerPackageDto?> AssignPackageToCustomerAsync(CreateCustomerPackageDto dto, int createdBy)
        {
            var existing = await _repo.GetByCustomerAndPackageAsync(dto.Customer_id, dto.Package_id);
            if (existing != null)
            {
                if (!existing.Is_deleted)
                    return null; // đã tồn tại active

                // khôi phục bản ghi đã xóa mềm
                existing.Is_deleted = false;
                existing.Is_active = dto.Is_active;
                existing.Updated_by = createdBy;
                await _repo.UpdateAsync(existing);
                return ToDto(await _repo.GetByIdAsync(existing.Id, includeDetails: true) ?? existing);
            }

            var entity = new Customer_package
            {
                Customer_id = dto.Customer_id,
                Package_id = dto.Package_id,
                Is_active = dto.Is_active,
                Created_by = createdBy,
                Updated_by = createdBy
            };
            var created = await _repo.AddAsync(entity);
            return ToDto(await _repo.GetByIdAsync(created.Id, includeDetails: true) ?? created);
        }

        public async Task<bool> UnassignPackageFromCustomerAsync(int id, int deletedBy)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;
            entity.Updated_by = deletedBy;
            await _repo.DeleteAsync(id);
            return true;
        }

        public async Task<bool> UpdateAsync(int id, UpdateCustomerPackageDto dto, int updatedBy)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;
            entity.Is_active = dto.Is_active;
            entity.Updated_by = updatedBy;
            await _repo.UpdateAsync(entity);
            return true;
        }

        public async Task<bool> ExistsAsync(int customerId, int packageId)
        {
            return await _repo.ExistsAsync(customerId, packageId);
        }

        private static CustomerPackageDto ToDto(Customer_package x) => new()
        {
            Id = x.Id,
            Customer_id = x.Customer_id,
            Customer_name = x.Customer?.Full_name ?? string.Empty,
            Package_id = x.Package_id,
            Package_code = x.Package?.Package_code ?? string.Empty,
            Package_name = x.Package?.Package_name ?? string.Empty,
            Is_active = x.Is_active,
            Created_at = x.Created_at
        };
    }
}
