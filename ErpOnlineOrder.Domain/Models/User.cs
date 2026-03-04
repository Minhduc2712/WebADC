using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Domain.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public bool Is_active { get; set; }

        public int Created_by { get; set; }

        public DateTime Created_at { get; set; }

        public int Updated_by { get; set; }

        public DateTime Updated_at { get; set; }

        public bool Is_deleted { get; set; }

        public virtual ICollection<User_role> User_roles { get; set; } = new List<User_role>();
        public virtual ICollection<User_permission> User_permissions { get; set; } = new List<User_permission>();

        public virtual Staff? Staff { get; set; }

        public virtual Customer? Customer { get; set; }
    }
}
