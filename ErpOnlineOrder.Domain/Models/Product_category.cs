namespace ErpOnlineOrder.Domain.Models
{
    public class Product_category
    {
        public int Id { get; set; }

        public int Product_id { get; set; }
        public virtual Product? Product { get; set; }
        public int Category_id { get; set; }
        public virtual Category? Category { get; set; }

        public int Created_by { get; set; }

        public DateTime Created_at { get; set; }

        public int Updated_by { get; set; }

        public DateTime Updated_at { get; set; }

        public bool Is_deleted { get; set; }
    }
}
