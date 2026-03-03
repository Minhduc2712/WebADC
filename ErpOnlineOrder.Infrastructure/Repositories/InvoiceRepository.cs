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

        public InvoiceRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        public async Task<Invoice?> GetByIdAsync(int id)
        {
            return await _context.Invoices
                .AsNoTracking()
                .Include(i => i.Customer)!.ThenInclude(c => c!.Customer_managements)
                .ThenInclude(cm => cm.Province)
                .Include(i => i.Staff)
                .Include(i => i.Order)
                .Include(i => i.Parent_invoice)
                .Include(i => i.Merged_into_invoice)
                .Include(i => i.Invoice_Details)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(i => i.Id == id && !i.Is_deleted);
        }

        public async Task<Invoice?> GetByCodeAsync(string code)
        {
            return await _context.Invoices
                .AsNoTracking()
                .Include(i => i.Customer)
                .Include(i => i.Invoice_Details)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(i => i.Invoice_code == code && !i.Is_deleted);
        }

        public async Task<IEnumerable<Invoice>> GetAllAsync()
        {
            return await _context.Invoices
                .AsNoTracking()
                .Include(i => i.Customer)!.ThenInclude(c => c!.Customer_managements)
                .ThenInclude(cm => cm.Province)
                .Include(i => i.Staff)
                .Include(i => i.Invoice_Details)
                    .ThenInclude(d => d.Product)
                .Where(i => !i.Is_deleted)
                .OrderByDescending(i => i.Created_at)
                .ToListAsync();
        }

        public async Task<PagedResult<Invoice>> GetPagedInvoicesAsync(InvoiceFilterRequest request, IEnumerable<int>? customerIds = null)
        {
            var query = _context.Invoices
                .AsNoTracking()
                .Include(i => i.Customer)!.ThenInclude(c => c!.Customer_managements)
                .ThenInclude(cm => cm.Province)
                .Include(i => i.Staff)
                .Include(i => i.Invoice_Details)
                    .ThenInclude(d => d.Product)
                .Where(i => !i.Is_deleted)
                .AsQueryable();

            if (customerIds != null)
            {
                var ids = customerIds.ToList();
                if (ids.Count > 0)
                    query = query.Where(i => ids.Contains(i.Customer_id));
                else
                    return new PagedResult<Invoice> { Items = new List<Invoice>(), Page = request.Page, PageSize = request.PageSize, TotalCount = 0 };
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(i => i.Status == request.Status);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.Trim().ToLowerInvariant();
                query = query.Where(i =>
                    (i.Invoice_code != null && i.Invoice_code.ToLower().Contains(search)) ||
                    (i.Customer != null && i.Customer.Full_name != null && i.Customer.Full_name.ToLower().Contains(search))
                );
            }

            query = query.OrderByDescending(i => i.Created_at);
            return await query.ToPagedListAsync(request);
        }

        public async Task<IEnumerable<Invoice>> GetByCustomerIdsAsync(IEnumerable<int> customerIds)
        {
            var ids = customerIds.ToList();
            if (ids.Count == 0) return new List<Invoice>();
            return await _context.Invoices
                .AsNoTracking()
                .Include(i => i.Customer)!.ThenInclude(c => c!.Customer_managements)
                .ThenInclude(cm => cm.Province)
                .Include(i => i.Staff)
                .Include(i => i.Invoice_Details)
                    .ThenInclude(d => d.Product)
                .Where(i => ids.Contains(i.Customer_id) && !i.Is_deleted)
                .OrderByDescending(i => i.Created_at)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Invoices
                .AsNoTracking()
                .Include(i => i.Customer)
                .Include(i => i.Invoice_Details)
                    .ThenInclude(d => d.Product)
                .Where(i => i.Customer_id == customerId && !i.Is_deleted)
                .OrderByDescending(i => i.Created_at)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByStatusAsync(string status)
        {
            return await _context.Invoices
                .AsNoTracking()
                .Include(i => i.Customer)
                .Include(i => i.Invoice_Details)
                .Where(i => i.Status == status && !i.Is_deleted)
                .OrderByDescending(i => i.Created_at)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetForMergeAsync(IEnumerable<int>? customerIds = null)
        {
            var excludedStatuses = new[] { "Completed", "Cancelled", "Merged" };
            var query = _context.Invoices
                .AsNoTracking()
                .Include(i => i.Customer)
                .Include(i => i.Invoice_Details).ThenInclude(d => d.Product)
                .Where(i => !i.Is_deleted && !excludedStatuses.Contains(i.Status ?? ""));

            if (customerIds != null)
            {
                var ids = customerIds.ToList();
                if (ids.Count > 0)
                    query = query.Where(i => ids.Contains(i.Customer_id));
                else
                    return new List<Invoice>();
            }

            return await query.OrderByDescending(i => i.Created_at).ToListAsync();
        }

        public async Task<IEnumerable<InvoiceSelectDto>> GetForWarehouseExportSelectAsync(IEnumerable<int>? customerIds = null)
        {
            var query = _context.Invoices
                .AsNoTracking()
                .Where(i => !i.Is_deleted && i.Status != "Cancelled")
                .AsQueryable();

            if (customerIds != null)
            {
                var ids = customerIds.ToList();
                if (ids.Count > 0)
                    query = query.Where(i => ids.Contains(i.Customer_id));
                else
                    return new List<InvoiceSelectDto>();
            }

            return await query
                .OrderByDescending(i => i.Created_at)
                .Select(i => new InvoiceSelectDto { Id = i.Id, Invoice_code = i.Invoice_code ?? "" })
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetChildInvoicesAsync(int parentInvoiceId)
        {
            return await _context.Invoices
                .AsNoTracking()
                .Include(i => i.Invoice_Details)
                    .ThenInclude(d => d.Product)
                .Where(i => i.Parent_invoice_id == parentInvoiceId && !i.Is_deleted)
                .ToListAsync();
        }

        public async Task AddAsync(Invoice invoice)
        {
            await _context.Invoices.AddAsync(invoice);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Invoice invoice)
        {
            _context.Invoices.Update(invoice);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice != null)
            {
                invoice.Is_deleted = true;
                invoice.Updated_at = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
