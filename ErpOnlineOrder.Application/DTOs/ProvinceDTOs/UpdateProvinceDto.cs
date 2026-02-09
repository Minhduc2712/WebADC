namespace ErpOnlineOrder.Application.DTOs.ProvinceDTOs
{
    public class UpdateProvinceDto
    {
        public int Id { get; set; }
        public string Province_code { get; set; } = string.Empty;
        public string Province_name { get; set; } = string.Empty;
        public int Region_id { get; set; }
    }
}
