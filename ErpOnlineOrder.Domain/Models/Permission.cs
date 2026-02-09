using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Domain.Models
{
    public class Permission
    {
        public int Id { get; set; }
        public string Permission_code { get; set; } = string.Empty;
        
        // public int Module_id { get; set; }
        // [ForeignKey("Module_id")]
        // public virtual Module Module { get; set; } = null!;

        // public int Action_id { get; set; }
        // [ForeignKey("Action_id")]
        // public virtual Action Action { get; set; } = null!;
        
        public int? Module_id { get; set; }
        public int? Action_id { get; set; }

        public int Created_by { get; set; }
        public DateTime Created_at { get; set; }
        public int Updated_by { get; set; }
        public DateTime Updated_at { get; set; }
        public bool Is_deleted { get; set; }

        public virtual ICollection<Role_permission> Role_Permissions { get; set; } = new List<Role_permission>();

        public virtual ICollection<User_permission> User_Permissions { get; set; } = new List<User_permission>();
    }
}
