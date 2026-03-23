using ErpOnlineOrder.Application.DTOs.StaffRegionRuleDTOs;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface IStaffRegionRuleService
    {
        Task<IEnumerable<StaffRegionRuleDto>> GetAllAsync(int? staffId = null, int? provinceId = null);
        Task<StaffRegionRuleDto?> GetByIdAsync(int id);
        Task<StaffRegionRuleDto> CreateAsync(CreateStaffRegionRuleDto dto, int createdBy);
        Task<StaffRegionRuleDto> UpdateAsync(int id, UpdateStaffRegionRuleDto dto, int updatedBy);
        Task DeleteAsync(int id);
    }
}
