using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public OrganizationRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        public async Task<Organization_information?> GetByIdAsync(int id)
        {
            return await _context.OrganizationInformations
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == id && !o.Is_deleted);
        }

        public async Task<Organization_information?> GetByCodeAsync(string organizationCode)
        {
            return await _context.OrganizationInformations
                .FirstOrDefaultAsync(o => o.Organization_code == organizationCode && !o.Is_deleted);
        }

        public async Task<IEnumerable<Organization_information>> GetAllAsync()
        {
            return await _context.OrganizationInformations
                .Include(o => o.Customer)
                .Where(o => !o.Is_deleted)
                .OrderBy(o => o.Organization_name)
                .ToListAsync();
        }

        public async Task<Organization_information?> GetByCustomerIdAsync(int customerId)
        {
            return await _context.OrganizationInformations
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Customer_id == customerId && !o.Is_deleted);
        }

        public async Task<IEnumerable<Organization_information>> SearchAsync(string? keyword)
        {
            var query = _context.OrganizationInformations
                .Include(o => o.Customer)
                .Where(o => !o.Is_deleted);

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(o => 
                    o.Organization_name.Contains(keyword) ||
                    o.Organization_code.Contains(keyword) ||
                    o.Tax_number.ToString().Contains(keyword));
            }

            return await query.OrderBy(o => o.Organization_name).ToListAsync();
        }

        public async Task AddAsync(Organization_information organization)
        {
            await _context.OrganizationInformations.AddAsync(organization);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Organization_information organization)
        {
            _context.OrganizationInformations.Update(organization);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var organization = await _context.OrganizationInformations.FindAsync(id);
            if (organization != null)
            {
                organization.Is_deleted = true;
                organization.Updated_at = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}