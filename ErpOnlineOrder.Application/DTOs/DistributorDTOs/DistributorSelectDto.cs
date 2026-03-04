namespace ErpOnlineOrder.Application.DTOs.DistributorDTOs
{
    public class DistributorSelectDto
    {
        public int Id { get; set; }
        public string Distributor_code { get; set; } = string.Empty;
        public string Distributor_name { get; set; } = string.Empty;
        public string Display_text => string.IsNullOrEmpty(Distributor_name) ? Distributor_code : $"{Distributor_code} - {Distributor_name}";
    }
}
