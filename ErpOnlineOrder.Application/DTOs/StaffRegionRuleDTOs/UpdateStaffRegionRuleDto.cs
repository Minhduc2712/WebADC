using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Application.DTOs.StaffRegionRuleDTOs
{
    public class UpdateStaffRegionRuleDto
    {
        [Required(ErrorMessage = "Vui lòng chọn cán bộ")]
        public int Staff_id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tỉnh/thành")]
        public int Province_id { get; set; }

        public List<int>? Ward_ids { get; set; }
    }
}
