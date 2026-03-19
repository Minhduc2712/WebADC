using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Application.DTOs.StockDTOs
{
    public class CreateStockDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn kho")]
        public int Warehouse_id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn sản phẩm")]
        public int Product_id { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn không được âm")]
        public int Quantity { get; set; }
    }
}