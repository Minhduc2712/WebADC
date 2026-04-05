using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Application.DTOs.CustomerProductDTOs
{

    public class UpdateCustomerProductDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Id phải > 0")]
        public int Id { get; set; }

        [Range(0, 1000000, ErrorMessage = "Max_quantity phải từ 0 đến 1.000.000")]
        public int? Max_quantity { get; set; }

        public bool Is_active { get; set; }
    }
}
