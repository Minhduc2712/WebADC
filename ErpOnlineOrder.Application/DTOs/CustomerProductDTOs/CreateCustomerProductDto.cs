using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Application.DTOs.CustomerProductDTOs
{
    public class CreateCustomerProductDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Customer_id phải > 0")]
        public int Customer_id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Product_id phải > 0")]
        public int Product_id { get; set; }

        [Range(0, 1000000, ErrorMessage = "Max_quantity phải từ 0 đến 1.000.000")]
        public int? Max_quantity { get; set; }

        public bool Is_active { get; set; } = true;
    }
}
