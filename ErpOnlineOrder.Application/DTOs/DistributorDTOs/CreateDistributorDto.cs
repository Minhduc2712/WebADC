namespace ErpOnlineOrder.Application.DTOs.DistributorDTOs
{
    public class CreateDistributorDto
    {
        public string Distributor_code { get; set; } = null!;
        public string Distributor_name { get; set; } = null!;
        public string Distributor_address { get; set; } = null!;
        public string Distributor_phone { get; set; } = null!;
        public string Distributor_email { get; set; } = null!;
    }
}
