using ErpOnlineOrder.Application.DTOs.StaffRegionRuleDTOs;

namespace ErpOnlineOrder.WebMVC.Services.Interfaces
{
    public interface IStaffRegionRuleApiClient
    {
        Task<IEnumerable<StaffRegionRuleDto>> GetAllAsync(int? staffId = null, int? provinceId = null, CancellationToken cancellationToken = default);
        Task<StaffRegionRuleDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<(StaffRegionRuleDto? Data, string? Error)> CreateAsync(CreateStaffRegionRuleDto dto, CancellationToken cancellationToken = default);
        Task<(StaffRegionRuleDto? Data, string? Error)> UpdateAsync(int id, UpdateStaffRegionRuleDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
