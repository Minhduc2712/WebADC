using ErpOnlineOrder.Application.DTOs.StaffRegionRuleDTOs;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class StaffRegionRuleApiClient : BaseApiClient, IStaffRegionRuleApiClient
    {
        public StaffRegionRuleApiClient(IHttpClientFactory factory) : base(factory.CreateClient("ErpApi"))
        {
        }

        public async Task<IEnumerable<StaffRegionRuleDto>> GetAllAsync(int? staffId = null, int? provinceId = null, CancellationToken cancellationToken = default)
        {
            var query = new List<string>();
            if (staffId.HasValue) query.Add($"staffId={staffId.Value}");
            if (provinceId.HasValue) query.Add($"provinceId={provinceId.Value}");
            var path = query.Count > 0 ? "staffregionrule?" + string.Join("&", query) : "staffregionrule";
            return await GetAsync<IEnumerable<StaffRegionRuleDto>>(path, cancellationToken) ?? Array.Empty<StaffRegionRuleDto>();
        }

        public async Task<StaffRegionRuleDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await GetAsync<StaffRegionRuleDto>($"staffregionrule/{id}", cancellationToken);
        }

        public async Task<(StaffRegionRuleDto? Data, string? Error)> CreateAsync(CreateStaffRegionRuleDto dto, CancellationToken cancellationToken = default)
        {
            return await PostAsync<CreateStaffRegionRuleDto, StaffRegionRuleDto>("staffregionrule", dto, cancellationToken);
        }

        public async Task<(StaffRegionRuleDto? Data, string? Error)> UpdateAsync(int id, UpdateStaffRegionRuleDto dto, CancellationToken cancellationToken = default)
        {
            var (success, error) = await PutAsync($"staffregionrule/{id}", dto, cancellationToken);
            if (!success) return (null, error);
            var updated = await GetByIdAsync(id, cancellationToken);
            return (updated, null);
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync($"staffregionrule/{id}", cancellationToken);
        }
    }
}
