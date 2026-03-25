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

        private IQueryable<Organization_information> GetBaseQuery()
        {
            return _context.OrganizationInformations.AsNoTracking();
        }

        public async Task<Organization_information?> GetByIdAsync(int id)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Organization_information?> GetByCodeAsync(string organizationCode)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(o => o.Organization_code == organizationCode);
        }

        public async Task<bool> ExistsByCodeAsync(string code, int? excludeId = null)
        {
            var query = GetBaseQuery().Where(o => o.Organization_code == code);
            if (excludeId.HasValue)
                query = query.Where(o => o.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<bool> ExistsByTaxNumberAsync(string taxNumber, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(taxNumber)) return false;
            var query = _context.OrganizationInformations.AsNoTracking()
                .Where(o => o.Tax_number == taxNumber);
            if (excludeId.HasValue)
                query = query.Where(o => o.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Organization_information>> GetAllAsync()
        {
            return await GetBaseQuery()
                .OrderBy(o => o.Organization_name)
                .ToListAsync();
        }

        public async Task<Organization_information?> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Customers.AsNoTracking()
                .Where(c => c.Id == customerId)
                .Include(c => c.Organization_information)
                .Select(c => c.Organization_information)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Organization_information>> SearchAsync(string? keyword)
        {
            var query = GetBaseQuery();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(o => 
                    o.Organization_name.Contains(keyword) ||
                    o.Organization_code.Contains(keyword) ||
                    o.Tax_number.Contains(keyword));
            }

            return await query.OrderBy(o => o.Organization_name).ToListAsync();
        }

        public async Task AddAsync(Organization_information organization)
        {
            organization.Created_at = DateTime.Now;
            organization.Updated_at = DateTime.Now;
            await _context.OrganizationInformations.AddAsync(organization);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Organization_information organization)
        {
            organization.Updated_at = DateTime.Now;
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