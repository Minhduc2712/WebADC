namespace ErpOnlineOrder.Application.DTOs.EmailDTOs
{
    /// <summary>
    /// Đại diện cho một tác vụ gửi email được đưa vào hàng đợi.
    /// ActionType xác định loại email; các tham số Id truyền context cần thiết.
    /// </summary>
    public class EmailMessage
    {
        public EmailActionType ActionType { get; init; }

        // Dùng cho các email liên quan đến đơn hàng / xuất kho / khách hàng / nhân viên
        public int? PrimaryId { get; init; }
        public int? SecondaryId { get; init; }
        public int? TertiaryId { get; init; }

        // Dùng khi cần truyền danh sách (vd: gán nhiều sản phẩm)
        public List<int>? IdList { get; init; }

        // Dùng để truyền dữ liệu chuỗi tự do (vd: reset link)
        public string? Payload { get; init; }
    }

    public enum EmailActionType
    {
        OrderNotificationStaffAndAdmin,
        OrderNotificationCustomer,
        OrderConfirmedNotificationCustomer,
        OrderWaitingCustomerNotification,
        OrderUpdatedNotificationCustomer,
        OrderRejectedByCustomerNotification,
        WarehouseExportNotificationStaffAndAdmin,
        ExportDeliveryStatusToCustomer,
        CustomerInvoiceRequestNotification,
        CustomerRegistrationNotification,
        StaffReplacementNotification,
        StaffAssignmentNotification,
        ProductAssignedToCustomer,
        PackageAssignedToCustomer,
        PasswordReset,
        InvoiceSentToCustomer,
    }
}
