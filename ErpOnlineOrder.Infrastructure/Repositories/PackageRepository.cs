using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class PackageRepository : IPackageRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public PackageRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        private IQueryable<Package> GetBaseQuery()
        {
            return _context.Packages
                .Where(p => !p.Is_deleted)
                .Include(p => p.Organization_information)
                .Include(p => p.Region)
                .Include(p => p.Province)
                .Include(p => p.Ward)
                .Include(p => p.Package_products.Where(pp => !pp.Is_deleted))
                    .ThenInclude(pp => pp.Product)
                .AsNoTracking();
        }

        public async Task<Package?> GetByIdAsync(int id)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Package>> GetAllAsync()
        {
            return await GetBaseQuery().OrderBy(p => p.Package_code).ToListAsync();
        }

        public async Task<IEnumerable<Package>> GetByOrganizationAsync(int organizationId)
        {
            return await GetBaseQuery()
                .Where(p => p.Organization_information_id == organizationId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Package>> GetByRegionAsync(int regionId)
        {
            return await GetBaseQuery()
                .Where(p => p.Region_id == regionId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Package>> GetByProvinceAsync(int provinceId)
        {
            return await GetBaseQuery()
                .Where(p => p.Province_id == provinceId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Package>> GetByWardAsync(int wardId)
        {
            return await GetBaseQuery()
                .Where(p => p.Ward_id == wardId)
                .ToListAsync();
        }

        public async Task<bool> ExistsByCodeAsync(string code, int? excludeId = null)
        {
            var query = _context.Packages.Where(p => p.Package_code == code);
            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task AddAsync(Package package)
        {
            package.Created_at = DateTime.Now;
            package.Updated_at = DateTime.Now;
            await _context.Packages.AddAsync(package);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Package package)
        {
            package.Updated_at = DateTime.Now;
            _context.Packages.Update(package);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var package = await _context.Packages.FindAsync(id);
            if (package != null)
            {
                package.Is_deleted = true;
                package.Updated_at = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Package_product?> GetPackageProductAsync(int packageId, int productId)
        {
            return await _context.PackageProducts
                .FirstOrDefaultAsync(pp => pp.Package_id == packageId && pp.Product_id == productId && !pp.Is_deleted);
        }

        public async Task AddPackageProductAsync(Package_product packageProduct)
        {
            packageProduct.Created_at = DateTime.Now;
            packageProduct.Updated_at = DateTime.Now;
            await _context.PackageProducts.AddAsync(packageProduct);
            await _context.SaveChangesAsync();
        }

        public async Task RemovePackageProductAsync(Package_product packageProduct)
        {
            packageProduct.Is_deleted = true;
            packageProduct.Updated_at = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }
}
