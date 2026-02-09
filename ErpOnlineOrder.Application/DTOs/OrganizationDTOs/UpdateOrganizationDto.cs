namespace ErpOnlineOrder.Application.DTOs.OrganizationDTOs
{
    public class UpdateOrganizationDto
    {
        public int Id { get; set; }
        public string Organization_code { get; set; } = string.Empty;
        public string Organization_name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int Tax_number { get; set; }
        public string? Recipient_name { get; set; }
        public int Recipient_phone { get; set; }
        public string? Recipient_address { get; set; }
    }
}
