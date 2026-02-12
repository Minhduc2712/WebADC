namespace ErpOnlineOrder.Domain.Models
{
    public class SystemSetting
    {
        public int Id { get; set; }
        public string SettingKey { get; set; } = null!;
        public string SettingValue { get; set; } = "";
        public string? Description { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
