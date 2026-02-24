using System.Text.Json.Serialization;

namespace ErpOnlineOrder.Application.DTOs.PermissionDTOs
{
    public class PermissionDto
    {
        public int Id { get; set; }
        public string Permission_code { get; set; } = string.Empty;
        public string Module_name { get; set; } = string.Empty;
        public string Action_name { get; set; } = string.Empty;
        public int? Parent_id { get; set; }
        public bool Is_special { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<PermissionDto>? Children { get; set; }
    }
}
