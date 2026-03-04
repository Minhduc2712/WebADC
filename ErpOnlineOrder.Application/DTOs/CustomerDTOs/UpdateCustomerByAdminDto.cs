namespace ErpOnlineOrder.Application.DTOs.CustomerDTOs
{
    public class UpdateCustomerByAdminDto
    {
        public int Id { get; set; }
        public string Customer_code { get; set; } = null!;
        public string Full_name { get; set; } = null!;
        public string Phone_number { get; set; } = null!;
        public string Address { get; set; } = null!;
    }
}
