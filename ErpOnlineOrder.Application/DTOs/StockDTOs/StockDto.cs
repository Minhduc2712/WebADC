using ErpOnlineOrder.Application.DTOs;

namespace ErpOnlineOrder.Application.DTOs.StockDTOs
{
    public class StockDto : IRecordPermissionDto
    {
        public int Id { get; set; }
        public int Warehouse_id { get; set; }
        public string Warehouse_name { get; set; } = string.Empty;
        public int Product_id { get; set; }
        public string Product_name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime Updated_at { get; set; }

        public bool AllowUpdate { get; set; }
        public bool AllowDelete { get; set; }
    }
}
