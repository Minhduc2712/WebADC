using ErpOnlineOrder.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class EmailRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public EmailRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }


    }
}
