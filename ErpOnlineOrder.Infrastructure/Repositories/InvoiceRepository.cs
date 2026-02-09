using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
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
                .Include(i => i.Customer)
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
                .Include(i => i.Customer)
                .Include(i => i.Invoice_Details)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(i => i.Invoice_code == code && !i.Is_deleted);
        }

        public async Task<IEnumerable<Invoice>> GetAllAsync()
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Staff)
                .Include(i => i.Invoice_Details)
                    .ThenInclude(d => d.Product)
                .Where(i => !i.Is_deleted)
                .OrderByDescending(i => i.Created_at)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Invoices
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
                .Include(i => i.Customer)
                .Include(i => i.Invoice_Details)
                .Where(i => i.Status == status && !i.Is_deleted)
                .OrderByDescending(i => i.Created_at)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetChildInvoicesAsync(int parentInvoiceId)
        {
            return await _context.Invoices
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
