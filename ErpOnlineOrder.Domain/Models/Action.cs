using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Domain.Models
{
    public class Action
    {
        public int Id { get; set; }

        public string Action_code { get; set; }
        public string Action_name { get; set; }
        public int Created_by { get; set; }
        public DateTime Created_at { get; set; }
        public int Updated_by { get; set; }
        public DateTime Updated_at { get; set; }
        public bool Is_deleted { get; set; }

        public virtual Permission? Permission { get; set; }
    }


}
