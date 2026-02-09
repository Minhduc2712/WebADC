using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.DTOs.EmailDTOs
{
    public class EmailSettingDTO
    {
        public string Host { get; set; } = null!;
        public int Port { get; set; }
        public bool UseSsl { get; set; }
        public string FromName { get; set; } = null!;
        public string FromEmail { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
