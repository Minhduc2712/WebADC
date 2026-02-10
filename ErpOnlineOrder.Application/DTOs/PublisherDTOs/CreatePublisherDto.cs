namespace ErpOnlineOrder.Application.DTOs.PublisherDTOs
{
    public class CreatePublisherDto
    {
        public string Publisher_code { get; set; } = null!;
        public string Publisher_name { get; set; } = null!;
        public string? Publisher_address { get; set; }
        public string? Publisher_phone { get; set; }
        public string? Publisher_email { get; set; }
    }
}
