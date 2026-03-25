namespace ErpOnlineOrder.Application.DTOs.CustomerDTOs
{
    /// <summary>
    /// Trả về thông tin đơn vị (do admin quản lý, chỉ đọc) + thông tin người nhận của khách hàng (có thể chỉnh sửa).
    /// </summary>
    public class CustomerOrganizationViewDto
    {
        public int Customer_id { get; set; }

        // Đơn vị đang chọn
        public int Organization_information_id { get; set; }
        public string Organization_code { get; set; } = string.Empty;
        public string Organization_name { get; set; } = string.Empty;
        public string Organization_address { get; set; } = string.Empty;
        public string Tax_number { get; set; } = string.Empty;

        // Thông tin người nhận của khách hàng
        public string? Recipient_name { get; set; }
        public string? Recipient_phone { get; set; }
        public string? Recipient_address { get; set; }
    }
}
