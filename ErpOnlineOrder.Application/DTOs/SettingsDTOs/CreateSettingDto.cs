using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Application.DTOs.SettingsDTOs
{
    public class CreateSettingDto
    {
        [Required(ErrorMessage = "Mã cài đặt không được để trống")]
        [MaxLength(100)]
        public string SettingKey { get; set; } = "";

        public string SettingValue { get; set; } = "";

        [MaxLength(200)]
        public string? Description { get; set; }
    }

    public class UpdateSettingDto
    {
        public string SettingValue { get; set; } = "";
        [MaxLength(200)]
        public string? Description { get; set; }
    }
}
