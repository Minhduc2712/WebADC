using ErpOnlineOrder.Application.DTOs.StaffRegionRuleDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Services
{
    public class StaffRegionRuleService : IStaffRegionRuleService
    {
        private readonly IStaffRegionRuleRepository _staffRegionRuleRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IProvinceRepository _provinceRepository;
        private readonly IWardRepository _wardRepository;

        public StaffRegionRuleService(
            IStaffRegionRuleRepository staffRegionRuleRepository,
            IStaffRepository staffRepository,
            IProvinceRepository provinceRepository,
            IWardRepository wardRepository)
        {
            _staffRegionRuleRepository = staffRegionRuleRepository;
            _staffRepository = staffRepository;
            _provinceRepository = provinceRepository;
            _wardRepository = wardRepository;
        }

        public async Task<IEnumerable<StaffRegionRuleDto>> GetAllAsync(int? staffId = null, int? provinceId = null)
        {
            IEnumerable<Staff_region_rule> rules;
            if (staffId.HasValue)
                rules = await _staffRegionRuleRepository.GetByStaffAsync(staffId.Value);
            else if (provinceId.HasValue)
                rules = await _staffRegionRuleRepository.GetByProvinceAsync(provinceId.Value);
            else
                rules = await _staffRegionRuleRepository.GetAllAsync();

            var ruleList = rules.ToList();
            var wardNames = await BuildWardNamesAsync(ruleList.SelectMany(r => r.Ward_ids ?? new()).Distinct());
            return ruleList.Select(r => MapToDto(r, wardNames));
        }

        public async Task<StaffRegionRuleDto?> GetByIdAsync(int id)
        {
            var rule = await _staffRegionRuleRepository.GetByIdAsync(id);
            if (rule == null) return null;
            var wardNames = await BuildWardNamesAsync(rule.Ward_ids ?? new());
            return MapToDto(rule, wardNames);
        }

        public async Task<StaffRegionRuleDto> CreateAsync(CreateStaffRegionRuleDto dto, int createdBy)
        {
            if (await _staffRegionRuleRepository.ExistsByStaffAndProvinceAsync(dto.Staff_id, dto.Province_id))
                throw new InvalidOperationException("Cán bộ này đã có quy tắc cho tỉnh/thành đó. Vui lòng chỉnh sửa quy tắc hiện có.");

            var newWardIds = dto.Ward_ids?.Where(id => id > 0).Distinct().ToList() ?? new List<int>();

            if (newWardIds.Count > 0)
            {
                // Kiểm tra ward không bị đồng thời thuộc rule khác cùng tỉnh
                var existingRules = await _staffRegionRuleRepository.GetByProvinceAsync(dto.Province_id);
                var usedWardIds = existingRules.SelectMany(r => r.Ward_ids ?? new()).ToHashSet();
                var overlapping = newWardIds.Where(wId => usedWardIds.Contains(wId)).ToList();
                if (overlapping.Any())
                {
                    var names = await BuildWardNamesAsync(overlapping);
                    var nameStr = string.Join(", ", overlapping.Select(id => names.GetValueOrDefault(id, id.ToString())));
                    throw new InvalidOperationException($"Các phường/xã sau đã có cán bộ phụ trách: {nameStr}");
                }
            }

            var rule = new Staff_region_rule
            {
                Staff_id = dto.Staff_id,
                Province_id = dto.Province_id,
                Ward_ids = newWardIds.Count > 0 ? newWardIds : null,
                Created_by = createdBy,
                Created_at = DateTime.Now,
                Updated_by = createdBy,
                Updated_at = DateTime.Now,
                Is_deleted = false
            };

            var created = await _staffRegionRuleRepository.AddAsync(rule);
            var loaded = await _staffRegionRuleRepository.GetByIdAsync(created.Id);
            var wardNames = await BuildWardNamesAsync(loaded!.Ward_ids ?? new());
            return MapToDto(loaded, wardNames);
        }

        public async Task<StaffRegionRuleDto> UpdateAsync(int id, UpdateStaffRegionRuleDto dto, int updatedBy)
        {
            var rule = await _staffRegionRuleRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Không tìm thấy quy tắc #{id}");

            if (await _staffRegionRuleRepository.ExistsByStaffAndProvinceAsync(dto.Staff_id, dto.Province_id, excludeId: id))
                throw new InvalidOperationException("Cán bộ này đã có quy tắc cho tỉnh/thành đó.");

            var newWardIds = dto.Ward_ids?.Where(wid => wid > 0).Distinct().ToList() ?? new List<int>();

            if (newWardIds.Count > 0)
            {
                var existingRules = (await _staffRegionRuleRepository.GetByProvinceAsync(dto.Province_id))
                    .Where(r => r.Id != id);
                var usedWardIds = existingRules.SelectMany(r => r.Ward_ids ?? new()).ToHashSet();
                var overlapping = newWardIds.Where(wId => usedWardIds.Contains(wId)).ToList();
                if (overlapping.Any())
                {
                    var names = await BuildWardNamesAsync(overlapping);
                    var nameStr = string.Join(", ", overlapping.Select(wid => names.GetValueOrDefault(wid, wid.ToString())));
                    throw new InvalidOperationException($"Các phường/xã sau đã có cán bộ phụ trách: {nameStr}");
                }
            }

            rule.Staff_id = dto.Staff_id;
            rule.Province_id = dto.Province_id;
            rule.Ward_ids = newWardIds.Count > 0 ? newWardIds : null;
            rule.Updated_by = updatedBy;
            rule.Updated_at = DateTime.Now;

            await _staffRegionRuleRepository.UpdateAsync(rule);

            var loaded = await _staffRegionRuleRepository.GetByIdAsync(id);
            var wardNames = await BuildWardNamesAsync(loaded!.Ward_ids ?? new());
            return MapToDto(loaded, wardNames);
        }

        public async Task DeleteAsync(int id)
        {
            _ = await _staffRegionRuleRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Không tìm thấy quy tắc #{id}");
            await _staffRegionRuleRepository.DeleteAsync(id);
        }

        private async Task<Dictionary<int, string>> BuildWardNamesAsync(IEnumerable<int> wardIds)
        {
            var ids = wardIds.ToList();
            if (ids.Count == 0) return new Dictionary<int, string>();
            return (await _wardRepository.GetManyByIdsAsync(ids))
                .ToDictionary(w => w.Id, w => w.Ward_name ?? w.Id.ToString());
        }

        private static StaffRegionRuleDto MapToDto(Staff_region_rule rule, Dictionary<int, string> wardNames) => new()
        {
            Id = rule.Id,
            Staff_id = rule.Staff_id,
            Staff_name = rule.Staff?.Full_name,
            Staff_code = rule.Staff?.Staff_code,
            Province_id = rule.Province_id,
            Province_name = rule.Province?.Province_name,
            Ward_ids = rule.Ward_ids ?? new List<int>(),
            Ward_names = (rule.Ward_ids ?? new List<int>())
                .Select(id => wardNames.GetValueOrDefault(id, id.ToString())).ToList(),
            Created_at = rule.Created_at,
            Updated_at = rule.Updated_at
        };
    }
}
