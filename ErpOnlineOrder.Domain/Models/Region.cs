using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Domain.Models
{
    public class Region
    {
        public int Id { get; set; }

        [Required]
        public string Region_code { get; set; }

        [Required]
        public string Region_name { get; set; }

        public int Created_by { get; set; }

        public DateTime Created_at { get; set; }

        public int Updated_by { get; set; }

        public DateTime Updated_at { get; set; }    

        public bool Is_deleted { get; set; }

        public virtual ICollection<Province>? Provinces { get; set; }
    }
}
