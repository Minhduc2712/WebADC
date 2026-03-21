using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Application.DTOs.WardDTOs
{
    public class CreateWardDto
    {
        [Required]
        public string Ward_code { get; set; } = null!;

        [Required]
        public string Ward_name { get; set; } = null!;

        [Required]
        public int Province_id { get; set; }
    }
}
