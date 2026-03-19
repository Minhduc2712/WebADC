namespace ErpOnlineOrder.Application.DTOs.OrderDTOs
{
    public class ConfirmOrderDto
    {
        public int OrderId { get; set; }
        public int Updated_by { get; set; }
        public string? Notify_method { get; set; } = "email";
        public List<ConfirmOrderItemDto> Approved_items { get; set; } = new();
    }

    public class ConfirmOrderItemDto
    {
        public int Product_id { get; set; }
        public int Quantity { get; set; }
        public bool Is_selected { get; set; }
    }

    public class ConfirmOrderResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Is_split { get; set; }
        public int? Child_order_id { get; set; }
        public string? Child_order_code { get; set; }
        public int? Invoice_id { get; set; }
        public string? Invoice_code { get; set; }
        public int? Warehouse_export_id { get; set; }
        public string? Warehouse_export_code { get; set; }
    }
}
