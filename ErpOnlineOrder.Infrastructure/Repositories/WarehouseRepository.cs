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

        public async Task<Warehouse?> GetByIdAsync(int id)
        {
            return await _context.Warehouses
                .AsNoTracking()
                .Include(w => w.Province)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<Warehouse?> GetByCodeAsync(string code)
        {
            return await _context.Warehouses
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Warehouse_code == code);
        }

        public async Task<Warehouse?> GetByNameAsync(string name)
        {
            return await _context.Warehouses
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Warehouse_name == name);
        }

        public async Task<IEnumerable<Warehouse>> GetAllAsync()
        {
            return await _context.Warehouses
                .AsNoTracking()
                .Include(w => w.Province)
                .OrderBy(w => w.Warehouse_name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Warehouse>> GetByProvinceIdAsync(int provinceId)
        {
            return await _context.Warehouses
                .AsNoTracking()
                .Include(w => w.Province)
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
            var query = _context.Warehouses
                .AsNoTracking()
                .OrderBy(w => w.Warehouse_name);
            return await ProjectToWarehouseSelectDto(query).ToListAsync();
        }

        public async Task<Warehouse> AddAsync(Warehouse warehouse)
        {
            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();
            return warehouse;
        }

        public async Task UpdateAsync(Warehouse warehouse)
        {
            _context.Warehouses.Update(warehouse);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse != null)
            {
                warehouse.Is_deleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
