using System.ComponentModel.DataAnnotations.Schema;

namespace ErpOnlineOrder.Domain.Models
{
    public class Permission
    {
        public int Id { get; set; }
        public string Permission_code { get; set; } = string.Empty;

        public int? Parent_id { get; set; }
        [ForeignKey("Parent_id")]
        public virtual Permission? Parent { get; set; }
        public virtual ICollection<Permission> Children { get; set; } = new List<Permission>();

        public bool Is_special { get; set; }

        public int Created_by { get; set; }
        public DateTime Created_at { get; set; }
        public int Updated_by { get; set; }
        public DateTime Updated_at { get; set; }
        public bool Is_deleted { get; set; }

        public virtual ICollection<Role_permission> Role_Permissions { get; set; } = new List<Role_permission>();
        public virtual ICollection<User_permission> User_Permissions { get; set; } = new List<User_permission>();
    }
}
