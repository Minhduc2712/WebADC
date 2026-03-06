using AutoMapper;
using AutoMapper.QueryableExtensions;
using ErpOnlineOrder.Application.DTOs.WarehouseDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class WarehouseRepository : IWarehouseRepository
    {
        private readonly ErpOnlineOrderDbContext _context;
        private readonly IMapper _mapper;

        public WarehouseRepository(ErpOnlineOrderDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private IQueryable<Warehouse> GetBaseQuery(bool includeDetails = false)
        {
            var query = _context.Warehouses.AsNoTracking();
            if (includeDetails)
            {
                query = query.Include(w => w.Province);
            }
            return query;
        }

        public async Task<Warehouse?> GetByIdAsync(int id)
        {
            return await GetBaseQuery(true).FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<Warehouse?> GetByCodeAsync(string code)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(w => w.Warehouse_code == code);
        }

        public async Task<bool> ExistsByCodeAsync(string code, int? excludeId = null)
        {
            var query = GetBaseQuery().Where(w => w.Warehouse_code == code);
            if (excludeId.HasValue)
                query = query.Where(w => w.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<Warehouse?> GetByNameAsync(string name)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(w => w.Warehouse_name == name);
        }

        public async Task<IEnumerable<Warehouse>> GetAllAsync()
        {
            return await GetBaseQuery(true)
                .OrderBy(w => w.Warehouse_name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Warehouse>> GetByProvinceIdAsync(int provinceId)
        {
            return await GetBaseQuery(true)
                .Where(w => w.Province_id == provinceId)
                .OrderBy(w => w.Warehouse_name)
                .ToListAsync();
        }

        private IQueryable<WarehouseSelectDto> ProjectToWarehouseSelectDto(IQueryable<Warehouse> query)
        {
            return query.ProjectTo<WarehouseSelectDto>(_mapper.ConfigurationProvider);
        }

        public async Task<IEnumerable<WarehouseSelectDto>> GetForSelectAsync()
        {
            var query = GetBaseQuery().OrderBy(w => w.Warehouse_name);
            return await ProjectToWarehouseSelectDto(query).ToListAsync();
        }

        public async Task<Warehouse> AddAsync(Warehouse warehouse)
        {
            warehouse.Created_at = DateTime.Now;
            warehouse.Updated_at = DateTime.Now;
            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();
            return warehouse;
        }

        public async Task UpdateAsync(Warehouse warehouse)
        {
            warehouse.Updated_at = DateTime.Now;
            _context.Warehouses.Update(warehouse);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse != null)
            {
                warehouse.Is_deleted = true;
                warehouse.Updated_at = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}
