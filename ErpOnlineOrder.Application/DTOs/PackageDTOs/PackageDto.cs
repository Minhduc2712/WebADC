namespace ErpOnlineOrder.Application.DTOs.PackageDTOs
{
    public class PackageDto
    {
        public int Id { get; set; }
        public string Package_code { get; set; } = string.Empty;
        public string Package_name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? Organization_information_id { get; set; }
        public string? Organization_name { get; set; }
        public int? Region_id { get; set; }
        public string? Region_name { get; set; }
        public int? Province_id { get; set; }
        public string? Province_name { get; set; }
        public int? Ward_id { get; set; }
        public string? Ward_name { get; set; }
        public bool Is_active { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }

        public List<PackageProductDto> Products { get; set; } = new();
    }
}
