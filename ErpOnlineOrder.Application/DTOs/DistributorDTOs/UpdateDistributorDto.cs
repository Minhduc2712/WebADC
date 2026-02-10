namespace ErpOnlineOrder.Application.DTOs.DistributorDTOs
{
    public class UpdateDistributorDto
    {
        public int Id { get; set; }
        public string Distributor_code { get; set; } = null!;
        public string Distributor_name { get; set; } = null!;
        public string Distributor_address { get; set; } = null!;
        public string Distributor_phone { get; set; } = null!;
        public string Distributor_email { get; set; } = null!;
    }
}