namespace ErpOnlineOrder.Application.DTOs.WardDTOs
{
    public class WardDTO
    {
        public int Id { get; set; }
        public string Ward_code { get; set; } = string.Empty;
        public string Ward_name { get; set; } = string.Empty;
        public int Province_id { get; set; }
        public string Province_name { get; set; } = string.Empty;
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
    }
}
