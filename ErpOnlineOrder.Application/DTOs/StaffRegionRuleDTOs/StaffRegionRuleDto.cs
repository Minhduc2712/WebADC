namespace ErpOnlineOrder.Application.DTOs.StaffRegionRuleDTOs
{
    public class StaffRegionRuleDto
    {
        public int Id { get; set; }
        public int Staff_id { get; set; }
        public string? Staff_name { get; set; }
        public string? Staff_code { get; set; }
        public int Province_id { get; set; }
        public string? Province_name { get; set; }
        public List<int> Ward_ids { get; set; } = new();
        public List<string> Ward_names { get; set; } = new();
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
    }
}
