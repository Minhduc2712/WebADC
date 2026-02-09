namespace ErpOnlineOrder.Application.DTOs.AdminDTOs
{
    public class UpdateStaffAccountDto
    {
        public int User_id { get; set; }
        public string? Email { get; set; }
        public string? Full_name { get; set; }
        public string? Phone_number { get; set; }
        public bool? Is_active { get; set; }
        public List<int>? Role_ids { get; set; }
    }
}
