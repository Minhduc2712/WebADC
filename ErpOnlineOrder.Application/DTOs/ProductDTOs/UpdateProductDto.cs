namespace ErpOnlineOrder.Application.DTOs.ProductDTOs
{
    public class UpdateProductDto
    {
        public int Id { get; set; }
        public string Product_code { get; set; } = null!;
        public string Product_name { get; set; } = null!;
        public string? Product_price { get; set; }
        public string? Product_link { get; set; }
        public string? Product_description { get; set; }
        public decimal? Tax_rate { get; set; }
        public int? Cover_type_id { get; set; }
        public int? Publisher_id { get; set; }
        public int? Distributor_id { get; set; }
        public int[]? CategoryIds { get; set; }
        public int[]? AuthorIds { get; set; }
    }
}
