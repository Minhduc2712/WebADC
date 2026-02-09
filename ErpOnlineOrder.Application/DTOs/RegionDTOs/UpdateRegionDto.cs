namespace ErpOnlineOrder.Application.DTOs.RegionDTOs
{
    public class UpdateRegionDto
    {
        public int Id { get; set; }
        public string Region_code { get; set; } = string.Empty;
        public string Region_name { get; set; } = string.Empty;
    }
}
