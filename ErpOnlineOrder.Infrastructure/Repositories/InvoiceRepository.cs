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

        private static IQueryable<InvoiceSelectDto> ProjectToInvoiceSelectDto(IQueryable<Invoice> query)
        {
            return query.Select(i => new InvoiceSelectDto
            {
                Id = i.Id,
                Invoice_code = i.Invoice_code ?? ""
            });
        }

        private static IQueryable<InvoiceDto> ProjectToInvoiceDto(IQueryable<Invoice> query)
        {
            return query.Select(i => new InvoiceDto
            {
                Id = i.Id,
                Invoice_code = i.Invoice_code ?? "",
                Invoice_date = i.Invoice_date,
                Customer_id = i.Customer_id,
                Customer_name = i.Customer != null ? (i.Customer.Full_name ?? "") : "",
                Province_id = i.Customer != null
                    ? i.Customer.Customer_managements
                        .Where(cm => cm.Province != null)
                        .Select(cm => (int?)cm.Province_id)
                        .FirstOrDefault()
                    : null,
                Province_name = i.Customer != null
                    ? i.Customer.Customer_managements
                        .Where(cm => cm.Province != null)
                        .Select(cm => cm.Province!.Province_name)
                        .FirstOrDefault()
                    : null,
                Total_amount = i.Total_amount,
                Tax_amount = i.Tax_amount,
                Status = i.Status ?? "",
                Parent_invoice_id = i.Parent_invoice_id,
                Parent_invoice_code = i.Parent_invoice != null ? i.Parent_invoice.Invoice_code : null,
                Details = i.Invoice_Details
                    .Select(d => new InvoiceDetailDto
                    {
                        Id = d.Id,
                        Product_id = d.Product_id,
                        Product_name = d.Product != null ? (d.Product.Product_name ?? "") : "",
                        Quantity = d.Quantity,
                        Unit_price = d.Unit_price,
                        Total_price = d.Total_price,
                        Tax_rate = d.Tax_rate
                    }).ToList()
            });
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
            return await _context.Invoices
                .AsNoTracking()
                .Include(i => i.Customer)
                .Include(i => i.Invoice_Details)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(i => i.Invoice_code == code);
        }

        public async Task<IEnumerable<InvoiceDto>> GetAllAsync()
        {
            var query = _context.Invoices
                .AsNoTracking()
                .OrderByDescending(i => i.Created_at);
            return await ProjectToInvoiceDto(query).ToListAsync();
        }

        public async Task<PagedResult<InvoiceDto>> GetPagedInvoicesAsync(InvoiceFilterRequest request, IEnumerable<int>? customerIds = null)
        {
            var query = _context.Invoices
                .AsNoTracking()
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
            var query = _context.Invoices
                .AsNoTracking()
                .Where(i => ids.Contains(i.Customer_id))
                .OrderByDescending(i => i.Created_at);
            return await ProjectToInvoiceDto(query).ToListAsync();
        }

        public async Task<IEnumerable<InvoiceDto>> GetByCustomerIdAsync(int customerId)
        {
            var query = _context.Invoices
                .AsNoTracking()
                .Where(i => i.Customer_id == customerId)
                .OrderByDescending(i => i.Created_at);
            return await ProjectToInvoiceDto(query).ToListAsync();
        }

        public async Task<IEnumerable<InvoiceDto>> GetByStatusAsync(string status)
        {
            var query = _context.Invoices
                .AsNoTracking()
                .Where(i => i.Status == status)
                .OrderByDescending(i => i.Created_at);
            return await ProjectToInvoiceDto(query).ToListAsync();
        }

        public async Task<IEnumerable<InvoiceDto>> GetForMergeAsync(IEnumerable<int>? customerIds = null)
        {
            var excludedStatuses = new[] { "Completed", "Cancelled", "Merged", "Split" };
            var query = _context.Invoices
                .AsNoTracking()
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
            var query = _context.Invoices
                .AsNoTracking()
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
            return await _context.Invoices
                .AsNoTracking()
                .Include(i => i.Invoice_Details)
                    .ThenInclude(d => d.Product)
                .Where(i => i.Parent_invoice_id == parentInvoiceId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByMergedIntoInvoiceIdAsync(int mergedInvoiceId)
        {
            return await _context.Invoices
                .Where(i => i.Merged_into_invoice_id == mergedInvoiceId)
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
