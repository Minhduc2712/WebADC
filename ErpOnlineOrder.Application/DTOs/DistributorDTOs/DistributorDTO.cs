
namespace ErpOnlineOrder.Application.DTOs.DistributorDTOs
{
    public class DistributorDto
    {
        public int Id { get; set; }
        public string Distributor_code { get; set; } = null!;
        public string Distributor_name { get; set; } = null!;
        public string Distributor_address { get; set; } = null!;
        public string Distributor_phone { get; set; } = null!;
        public string Distributor_email { get; set; } = null!;
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
    }

    
}