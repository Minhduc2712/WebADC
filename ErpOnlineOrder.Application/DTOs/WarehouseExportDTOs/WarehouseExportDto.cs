namespace ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs
{
    public class WarehouseExportDto
    {
        public int Id { get; set; }
        public string Warehouse_export_code { get; set; } = string.Empty;
        public DateTime Export_date { get; set; }
        
        public int Warehouse_id { get; set; }
        public string Warehouse_name { get; set; } = string.Empty;
        
        public int Invoice_id { get; set; }
        public string Invoice_code { get; set; } = string.Empty;
        
        public int? Order_id { get; set; }
        public string? Order_code { get; set; }
        
        public int Customer_id { get; set; }
        public string Customer_name { get; set; } = string.Empty;
        
        public int Staff_id { get; set; }
        public string Staff_name { get; set; } = string.Empty;
        
        public string? Carrier_name { get; set; }
        public string? Tracking_number { get; set; }
        public string Delivery_status { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        
        public int? Parent_export_id { get; set; }
        public string? Parent_export_code { get; set; }
        public int? Merged_into_export_id { get; set; }
        public string? Merged_into_export_code { get; set; }
        public string? Split_merge_note { get; set; }
        
        public decimal Total_amount { get; set; }
        public int Total_quantity { get; set; }
        
        public DateTime Created_at { get; set; }
        
        public List<WarehouseExportDetailDto> Details { get; set; } = new();
        public List<WarehouseExportDto> Child_exports { get; set; } = new();
    }
    public class WarehouseExportDetailDto
    {
        public int Id { get; set; }
        public int Warehouse_export_id { get; set; }
        public int Warehouse_id { get; set; }
        public string Warehouse_name { get; set; } = string.Empty;
        public int Product_id { get; set; }
        public string Product_code { get; set; } = string.Empty;
        public string Product_name { get; set; } = string.Empty;
        public int Quantity_shipped { get; set; }
        public decimal Unit_price { get; set; }
        public decimal Total_price { get; set; }
    }
}
