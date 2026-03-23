using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Application.DTOs.StaffRegionRuleDTOs
{
    public class CreateStaffRegionRuleDto
    {
        [Required(ErrorMessage = "Vui lòng chọn cán bộ")]
        public int Staff_id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tỉnh/thành")]
        public int Province_id { get; set; }

        /// <summary>Rỗng/null = phụ trách toàn tỉnh; có phần tử = chỉ phụ trách các phường/xã được liệt kê</summary>
        public List<int>? Ward_ids { get; set; }
    }
}
