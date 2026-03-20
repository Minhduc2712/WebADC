using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using ErpOnlineOrder.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class WarehouseExportRepository : IWarehouseExportRepository
    {
        private readonly ErpOnlineOrderDbContext _context;

        public WarehouseExportRepository(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        private IQueryable<Warehouse_export> GetBaseQuery()
        {
            return _context.WarehouseExports.AsNoTracking();
        }

        public async Task<Warehouse_export?> GetByIdAsync(int id)
        {
            return await GetBaseQuery()
                .Include(e => e.Warehouse)
                .Include(e => e.Invoice)
                .Include(e => e.Order)
                .Include(e => e.Customer)
                .Include(e => e.Staff)
                .Include(e => e.Parent_export)
                .Include(e => e.Merged_into_export)
                .Include(e => e.Warehouse_Export_Details)
                    .ThenInclude(d => d.Product)
                .Include(e => e.Warehouse_Export_Details)
                    .ThenInclude(d => d.Warehouse)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Warehouse_export?> GetByCodeAsync(string code)
        {
            return await GetBaseQuery()
                .Include(e => e.Warehouse)
                .Include(e => e.Invoice)
                .Include(e => e.Customer)
                .FirstOrDefaultAsync(e => e.Warehouse_export_code == code);
        }

        public async Task<IEnumerable<Warehouse_export>> GetAllAsync()
        {
            return await GetBaseQuery()
                .Include(e => e.Warehouse)
                .Include(e => e.Invoice)
                .Include(e => e.Customer)
                .Include(e => e.Staff)
                .Include(e => e.Warehouse_Export_Details)
                    .ThenInclude(d => d.Product)
                .OrderByDescending(e => e.Created_at)
                .ToListAsync();
        }

        public async Task<PagedResult<Warehouse_export>> GetPagedWarehouseExportsAsync(WarehouseExportFilterRequest request, IEnumerable<int>? customerIds = null)
        {
            var query = GetBaseQuery()
                .Include(e => e.Warehouse)
                .Include(e => e.Invoice)
                .Include(e => e.Customer)
                .Include(e => e.Staff)
                .Include(e => e.Warehouse_Export_Details)
                    .ThenInclude(d => d.Product)
                .AsQueryable();

            if (customerIds != null)
            {
                var ids = customerIds.ToList();
                if (ids.Count > 0)
                    query = query.Where(e => ids.Contains(e.Customer_id));
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(e => e.Status == request.Status);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.Trim().ToLowerInvariant();
                query = query.Where(e =>
                    (e.Warehouse_export_code != null && e.Warehouse_export_code.ToLower().Contains(search)) ||
                    (e.Customer != null && e.Customer.Full_name != null && e.Customer.Full_name.ToLower().Contains(search))
                );
            }

            query = query.OrderByDescending(e => e.Created_at);
            return await query.ToPagedListAsync(request);
        }

        public async Task<PagedResult<(Warehouse_export Export, int IndentLevel)>> GetPagedHierarchicalExportsAsync(WarehouseExportFilterRequest request, IEnumerable<int>? customerIds = null)
        {
            var parameters = new List<SqlParameter>();
            string filterSql = "WHERE e.Parent_export_id IS NULL AND e.Is_deleted = 0";

            if (customerIds != null)
            {
                var idsList = customerIds.ToList();
                if (idsList.Count > 0)
                {
                    var ids = string.Join(",", idsList);
                    filterSql += $" AND e.Customer_id IN ({ids})"; 
                }
                else
                {
                    return new PagedResult<(Warehouse_export, int)> { Items = new List<(Warehouse_export, int)>(), Page = request.Page, PageSize = request.PageSize, TotalCount = 0 };
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                filterSql += " AND e.Status = @status";
                parameters.Add(new SqlParameter("@status", request.Status));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = $"%{request.SearchTerm.Trim().ToLowerInvariant()}%";
                filterSql += " AND (LOWER(e.Warehouse_export_code) LIKE @search OR LOWER(c.Full_name) LIKE @search)";
                parameters.Add(new SqlParameter("@search", search));
            }

            int skip = (request.Page - 1) * request.PageSize;
            parameters.Add(new SqlParameter("@skip", skip));
            parameters.Add(new SqlParameter("@take", request.PageSize));

            string sql = $@"
                WITH ExportTree AS (
                    SELECT 
                        e.Id, e.Parent_export_id, e.Export_date,
                        0 AS IndentLevel,
                        e.Export_date AS RootExportDate,
                        CAST(RIGHT('0000000000' + CAST(e.Id AS VARCHAR(10)), 10) AS VARCHAR(MAX)) AS SortPath
                    FROM WarehouseExports e
                    LEFT JOIN Customers c ON e.Customer_id = c.Id
                    {filterSql}

                    UNION ALL

                    SELECT 
                        child.Id, child.Parent_export_id, child.Export_date,
                        et.IndentLevel + 1,
                        et.RootExportDate,
                        et.SortPath + '-' + CAST(RIGHT('0000000000' + CAST(child.Id AS VARCHAR(10)), 10) AS VARCHAR(MAX))
                    FROM WarehouseExports child
                    INNER JOIN ExportTree et ON child.Parent_export_id = et.Id
                    WHERE child.Is_deleted = 0
                )
                SELECT Id, IndentLevel, COUNT(1) OVER() AS TotalCount 
                FROM ExportTree
                ORDER BY RootExportDate DESC, SortPath ASC
                OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;
            ";

            var idList = new List<int>();
            var indentDict = new Dictionary<int, int>();
            int totalCount = 0;

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = sql;
                command.CommandType = System.Data.CommandType.Text;
                foreach (var p in parameters) command.Parameters.Add(p);

                if (command.Connection.State != System.Data.ConnectionState.Open)
                    await command.Connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int id = reader.GetInt32(0);
                        int indent = reader.GetInt32(1);
                        totalCount = reader.GetInt32(2);

                        idList.Add(id);
                        indentDict[id] = indent;
                    }
                }
            }

            if (idList.Count == 0)
                return new PagedResult<(Warehouse_export, int)> { Items = new List<(Warehouse_export, int)>(), Page = request.Page, PageSize = request.PageSize, TotalCount = 0 };

            var entities = await GetBaseQuery()
                .Include(e => e.Warehouse)
                .Include(e => e.Invoice)
                .Include(e => e.Customer)
                .Include(e => e.Staff)
                .Include(e => e.Warehouse_Export_Details)
                    .ThenInclude(d => d.Product)
                .Where(e => idList.Contains(e.Id))
                .ToListAsync();

            var sortedItems = new List<(Warehouse_export, int)>();
            foreach (var id in idList)
            {
                var entity = entities.FirstOrDefault(e => e.Id == id);
                if (entity != null)
                {
                    sortedItems.Add((entity, indentDict[id]));
                }
            }

            return new PagedResult<(Warehouse_export, int)>
            {
                Items = sortedItems,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<IEnumerable<Warehouse_export>> GetByInvoiceIdAsync(int invoiceId)
        {
            return await GetBaseQuery()
                .Include(e => e.Warehouse)
                .Include(e => e.Invoice)
                .Include(e => e.Warehouse_Export_Details)
                    .ThenInclude(d => d.Product)
                .Where(e => e.Invoice_id == invoiceId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Warehouse_export>> GetByCustomerIdsAsync(IEnumerable<int> customerIds)
        {
            var ids = customerIds.ToList();
            if (ids.Count == 0) return new List<Warehouse_export>();

            return await GetBaseQuery()
                .Include(e => e.Warehouse)
                .Include(e => e.Invoice)
                .Include(e => e.Customer)
                .Include(e => e.Staff)
                .Include(e => e.Warehouse_Export_Details)
                    .ThenInclude(d => d.Product)
                .Where(e => ids.Contains(e.Customer_id))
                .OrderByDescending(e => e.Created_at)
                .ToListAsync();
        }

        public async Task<IEnumerable<Warehouse_export>> GetByCustomerIdAsync(int customerId)
        {
            return await GetBaseQuery()
                .Include(e => e.Warehouse)
                .Include(e => e.Invoice)
                .Include(e => e.Warehouse_Export_Details)
                    .ThenInclude(d => d.Product)
                .Where(e => e.Customer_id == customerId)
                .OrderByDescending(e => e.Created_at)
                .ToListAsync();
        }

        public async Task<IEnumerable<Warehouse_export>> GetByWarehouseIdAsync(int warehouseId)
        {
            return await GetBaseQuery()
                .Include(e => e.Invoice)
                .Include(e => e.Customer)
                .Include(e => e.Warehouse_Export_Details)
                    .ThenInclude(d => d.Product)
                .Where(e => e.Warehouse_id == warehouseId)
                .OrderByDescending(e => e.Created_at)
                .ToListAsync();
        }

        public async Task<IEnumerable<Warehouse_export>> GetByStatusAsync(string status)
        {
            return await GetBaseQuery()
                .Include(e => e.Warehouse)
                .Include(e => e.Invoice)
                .Include(e => e.Customer)
                .Where(e => e.Status == status)
                .OrderByDescending(e => e.Created_at)
                .ToListAsync();
        }

        public async Task<IEnumerable<Warehouse_export>> GetChildExportsAsync(int parentExportId)
        {
            return await GetBaseQuery()
                .Include(e => e.Warehouse)
                .Include(e => e.Invoice)
                .Include(e => e.Warehouse_Export_Details)
                    .ThenInclude(d => d.Product)
                .Where(e => e.Parent_export_id == parentExportId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Warehouse_export>> GetByMergedIntoExportIdAsync(int mergedExportId)
        {
            return await _context.WarehouseExports
                .Where(e => e.Merged_into_export_id == mergedExportId)
                .ToListAsync();
        }

        public async Task<int> GetMaxChildSuffixAsync(string codePrefix)
        {
            var codes = await _context.WarehouseExports
                .IgnoreQueryFilters()
                .Where(e => e.Warehouse_export_code.StartsWith(codePrefix))
                .Select(e => e.Warehouse_export_code)
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

        public async Task AddAsync(Warehouse_export export)
        {
            export.Created_at = DateTime.Now;
            export.Updated_at = DateTime.Now;
            await _context.WarehouseExports.AddAsync(export);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Warehouse_export export)
        {
            export.Updated_at = DateTime.Now;
            var tracked = await _context.WarehouseExports
                .Include(e => e.Warehouse_Export_Details)
                .FirstOrDefaultAsync(e => e.Id == export.Id);
            if (tracked == null) return;

            _context.Entry(tracked).CurrentValues.SetValues(export);

            // Sync detail changes (Split/UndoSplit modify details)
            if (export.Warehouse_Export_Details != null)
            {
                foreach (var detail in export.Warehouse_Export_Details)
                {
                    var trackedDetail = tracked.Warehouse_Export_Details
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
            var export = await _context.WarehouseExports.FindAsync(id);
            if (export != null)
            {
                export.Is_deleted = true;
                export.Updated_at = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}
