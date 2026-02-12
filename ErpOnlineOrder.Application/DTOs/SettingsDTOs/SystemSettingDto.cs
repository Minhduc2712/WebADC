namespace ErpOnlineOrder.Application.DTOs.SettingsDTOs
{
    public class SystemSettingDto
    {
        public int Id { get; set; }
        public string SettingKey { get; set; } = null!;
        public string SettingValue { get; set; } = "";
        public string? Description { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
