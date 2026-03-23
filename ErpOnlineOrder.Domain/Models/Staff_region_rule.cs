using System.ComponentModel.DataAnnotations.Schema;

namespace ErpOnlineOrder.Domain.Models
{
    public class Staff_region_rule
    {
        public int Id { get; set; }

        public int Staff_id { get; set; }

        [ForeignKey("Staff_id")]
        public virtual Staff? Staff { get; set; }

        public int Province_id { get; set; }

        [ForeignKey("Province_id")]
        public virtual Province? Province { get; set; }


        public List<int>? Ward_ids { get; set; }

        public int Created_by { get; set; }

        public DateTime Created_at { get; set; }

        public int Updated_by { get; set; }

        public DateTime Updated_at { get; set; }

        public bool Is_deleted { get; set; }
    }
}
