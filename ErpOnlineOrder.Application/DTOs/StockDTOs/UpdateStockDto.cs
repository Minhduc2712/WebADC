using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Application.DTOs.StockDTOs
{
    public class UpdateStockDto
    {
        public int Id { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn không được âm")]
        public int Quantity { get; set; }
    }
}
