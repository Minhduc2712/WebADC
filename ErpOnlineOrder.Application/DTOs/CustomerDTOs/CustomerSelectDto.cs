namespace ErpOnlineOrder.Application.DTOs.CustomerDTOs
{
    public class CustomerSelectDto
    {
        public int Id { get; set; }
        public string Customer_code { get; set; } = string.Empty;
        public string Full_name { get; set; } = string.Empty;
        public string Display_text => string.IsNullOrEmpty(Full_name) ? Customer_code : $"{Customer_code} - {Full_name}";
    }
}
