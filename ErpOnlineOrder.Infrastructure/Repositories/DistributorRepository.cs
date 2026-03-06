using AutoMapper;
using AutoMapper.QueryableExtensions;
using ErpOnlineOrder.Application.DTOs.DistributorDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class DistributorRepository : IDistributorRepository
    {
        private readonly ErpOnlineOrderDbContext _context;
        private readonly IMapper _mapper;

        public DistributorRepository(ErpOnlineOrderDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private IQueryable<Distributor> GetBaseQuery()
        {
            return _context.Distributors.AsNoTracking();
        }

        public async Task<Distributor?> GetByIdAsync(int id)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Distributor?> GetByCodeAsync(string code)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(d => d.Distributor_code == code);
        }

        public async Task<bool> ExistsByCodeAsync(string code, int? excludeId = null)
        {
            var query = GetBaseQuery().Where(d => d.Distributor_code == code);
            if (excludeId.HasValue)
                query = query.Where(d => d.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<Distributor?> GetByNameAsync(string name)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(d => d.Distributor_name == name);
        }

        public async Task<IEnumerable<Distributor>> GetAllAsync()
        {
            return await GetBaseQuery().ToListAsync();
        }

        private IQueryable<DistributorSelectDto> ProjectToDistributorSelectDto(IQueryable<Distributor> query)
        {
            return query.ProjectTo<DistributorSelectDto>(_mapper.ConfigurationProvider);
        }

        public async Task<IEnumerable<DistributorSelectDto>> GetForSelectAsync()
        {
            var query = GetBaseQuery().OrderBy(d => d.Distributor_name ?? d.Distributor_code ?? "");
            return await ProjectToDistributorSelectDto(query).ToListAsync();
        }

        public async Task AddAsync(Distributor distributor)
        {
            distributor.Created_at = DateTime.Now;
            distributor.Updated_at = DateTime.Now;
            await _context.Distributors.AddAsync(distributor);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Distributor distributor)
        {
            distributor.Updated_at = DateTime.Now;
            _context.Distributors.Update(distributor);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var distributor = await _context.Distributors.FindAsync(id);
            if (distributor != null)
            {
                distributor.Is_deleted = true;
                distributor.Updated_at = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}
