using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface IStaffRegionRuleRepository
    {
        Task<IEnumerable<Staff_region_rule>> GetAllAsync(bool includeDetails = true);
        Task<IEnumerable<Staff_region_rule>> GetByStaffAsync(int staffId);
        Task<IEnumerable<Staff_region_rule>> GetByProvinceAsync(int provinceId);
        Task<Staff_region_rule?> GetByIdAsync(int id);
        Task<Staff_region_rule?> FindByProvinceAndWardAsync(int provinceId, int? wardId);
        Task<bool> ExistsByStaffAndProvinceAsync(int staffId, int provinceId, int? excludeId = null);
        Task<Staff_region_rule> AddAsync(Staff_region_rule rule);
        Task UpdateAsync(Staff_region_rule rule);
        Task DeleteAsync(int id);
    }
}
