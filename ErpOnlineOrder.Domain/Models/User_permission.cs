using System.ComponentModel.DataAnnotations.Schema;

namespace ErpOnlineOrder.Domain.Models
{
    public class User_permission
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        
        public int PermissionId { get; set; }
        [ForeignKey("PermissionId")]
        public virtual Permission Permission { get; set; } = null!;
        public int? GrantedBy { get; set; }
        public DateTime GrantedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? Note { get; set; }
        
        public bool IsDeleted { get; set; }
    }
}
