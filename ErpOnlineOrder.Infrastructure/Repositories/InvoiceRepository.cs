using AutoMapper;
using AutoMapper.QueryableExtensions;
using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using ErpOnlineOrder.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly ErpOnlineOrderDbContext _context;
        private readonly IMapper _mapper;

        public InvoiceRepository(ErpOnlineOrderDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private IQueryable<InvoiceSelectDto> ProjectToInvoiceSelectDto(IQueryable<Invoice> query)
        {
            return query.ProjectTo<InvoiceSelectDto>(_mapper.ConfigurationProvider);
        }

        private IQueryable<InvoiceDto> ProjectToInvoiceDto(IQueryable<Invoice> query)
        {
            return query.ProjectTo<InvoiceDto>(_mapper.ConfigurationProvider);
        }

        private IQueryable<Invoice> GetBaseQuery()
        {
            return _context.Invoices.AsNoTracking();
        }
        
        public async Task<Invoice?> GetByIdAsync(int id)
        {
            return await GetBaseQuery()
                .Include(i => i.Customer)!.ThenInclude(c => c!.Customer_managements)
                .ThenInclude(cm => cm.Province)
                .Include(i => i.Staff)
                .Include(i => i.Order)
                .Include(i => i.Parent_invoice)
                .Include(i => i.Merged_into_invoice)
                .Include(i => i.Invoice_Details)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Invoice?> GetByIdForUpdateAsync(int id)
        {
            return await _context.Invoices
                .Include(i => i.Customer)!.ThenInclude(c => c!.Customer_managements)
                .ThenInclude(cm => cm.Province)
                .Include(i => i.Staff)
                .Include(i => i.Invoice_Details)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Invoice?> GetByCodeAsync(string code)
        {
            return await GetBaseQuery()
                .Include(i => i.Customer)
                .Include(i => i.Invoice_Details)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(i => i.Invoice_code == code);
        }

        public async Task<IEnumerable<InvoiceDto>> GetAllAsync()
        {
            var query = GetBaseQuery()
                .OrderByDescending(i => i.Created_at);
            return await ProjectToInvoiceDto(query).ToListAsync();
        }

        public async Task<PagedResult<InvoiceDto>> GetPagedInvoicesAsync(InvoiceFilterRequest request, IEnumerable<int>? customerIds = null)
        {
            var query = GetBaseQuery()
                .AsQueryable();

            if (customerIds != null && customerIds.Any())
            {
                query = query.Where(i => customerIds.Contains(i.Customer_id));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.Trim();
                query = query.Where(i => i.Invoice_code.Contains(search) || i.Customer.Full_name.Contains(search));
            }

            var projectedQuery = ProjectToInvoiceDto(query.OrderByDescending(i => i.Created_at));
            return await projectedQuery.ToPagedListAsync(request);
        }

        public async Task<IEnumerable<InvoiceDto>> GetByCustomerIdsAsync(IEnumerable<int> customerIds)
        {
            var ids = customerIds.ToList();
            if (ids.Count == 0) return new List<InvoiceDto>();
            var query = GetBaseQuery()
                .Where(i => ids.Contains(i.Customer_id))
                .OrderByDescending(i => i.Created_at);
            return await ProjectToInvoiceDto(query).ToListAsync();
        }

        public async Task<IEnumerable<InvoiceDto>> GetByCustomerIdAsync(int customerId)
        {
            var query = GetBaseQuery()
                .Where(i => i.Customer_id == customerId)
                .OrderByDescending(i => i.Created_at);
            return await ProjectToInvoiceDto(query).ToListAsync();
        }

        public async Task<IEnumerable<InvoiceDto>> GetByStatusAsync(string status)
        {
            var query = GetBaseQuery()
                .Where(i => i.Status == status)
                .OrderByDescending(i => i.Created_at);
            return await ProjectToInvoiceDto(query).ToListAsync();
        }

        public async Task<IEnumerable<InvoiceDto>> GetForMergeAsync(IEnumerable<int>? customerIds = null)
        {
            var excludedStatuses = new[] { "Completed", "Cancelled", "Merged", "Split" };
            var query = GetBaseQuery()
                .Where(i => !excludedStatuses.Contains(i.Status ?? ""));

            if (customerIds != null)
            {
                var ids = customerIds.ToList();
                if (ids.Count > 0)
                    query = query.Where(i => ids.Contains(i.Customer_id));
                else
                    return new List<InvoiceDto>();
            }

            query = query.OrderByDescending(i => i.Created_at);
            return await ProjectToInvoiceDto(query).ToListAsync();
        }

        public async Task<IEnumerable<InvoiceSelectDto>> GetForWarehouseExportSelectAsync(IEnumerable<int>? customerIds = null)
        {
            var query = GetBaseQuery()
                .Where(i => i.Status != "Cancelled")
                .AsQueryable();

            if (customerIds != null)
            {
                var ids = customerIds.ToList();
                if (ids.Count > 0)
                    query = query.Where(i => ids.Contains(i.Customer_id));
                else
                    return new List<InvoiceSelectDto>();
            }

            return await ProjectToInvoiceSelectDto(query.OrderByDescending(i => i.Created_at)).ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetChildInvoicesAsync(int parentInvoiceId)
        {
            return await GetBaseQuery()
                .Include(i => i.Invoice_Details)
                    .ThenInclude(d => d.Product)
                .Where(i => i.Parent_invoice_id == parentInvoiceId)
                .ToListAsync();
        }

        public async Task<int> GetMaxChildSuffixAsync(string codePrefix)
        {
            // IgnoreQueryFilters để tính cả bản ghi đã xóa mềm, tránh trùng mã
            var codes = await _context.Invoices
                .IgnoreQueryFilters()
                .Where(i => i.Invoice_code.StartsWith(codePrefix))
                .Select(i => i.Invoice_code)
                .ToListAsync();

            var max = 0;
            foreach (var code in codes)
            {
                var suffix = code.Substring(codePrefix.Length);
                if (int.TryParse(suffix, out var num) && num > max)
                    max = num;
            }
            return max;
        }

        public async Task<IEnumerable<Invoice>> GetByMergedIntoInvoiceIdAsync(int mergedInvoiceId)
        {
            return await _context.Invoices
                .Where(i => i.Merged_into_invoice_id == mergedInvoiceId)
                .ToListAsync();
        }

        public async Task AddAsync(Invoice invoice)
        {
            invoice.Created_at = DateTime.Now;
            invoice.Updated_at = DateTime.Now;
            await _context.Invoices.AddAsync(invoice);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Invoice invoice)
        {
            invoice.Updated_at = DateTime.Now;
            var tracked = await _context.Invoices
                .Include(i => i.Invoice_Details)
                .FirstOrDefaultAsync(i => i.Id == invoice.Id);
            if (tracked == null) return;

            _context.Entry(tracked).CurrentValues.SetValues(invoice);

            if (invoice.Invoice_Details != null)
            {
                foreach (var detail in invoice.Invoice_Details)
                {
                    var trackedDetail = tracked.Invoice_Details
                        .FirstOrDefault(d => d.Id == detail.Id);
                    if (trackedDetail != null)
                    {
                        _context.Entry(trackedDetail).CurrentValues.SetValues(detail);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice != null)
            {
                invoice.Is_deleted = true;
                invoice.Updated_at = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}
