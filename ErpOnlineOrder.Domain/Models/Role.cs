using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Domain.Models
{
    public class Role
    {
        public int Id { get; set; }

        [Required]
        public string Role_name { get; set; }
        public int Created_by { get; set; }
        public DateTime Created_at { get; set; }
        public int Updated_by { get; set; }
        public DateTime Updated_at { get; set; }
        public bool Is_deleted { get; set; }
        public virtual ICollection<User_role> User_roles { get; set; } = new List<User_role>();
        public virtual ICollection<Role_permission> Role_Permissions { get; set; } = new List<Role_permission>();
    }
}
