using AutoMapper;
using AutoMapper.QueryableExtensions;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Domain.Models;
using ErpOnlineOrder.Infrastructure.Persistence;
using ErpOnlineOrder.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErpOnlineOrder.Application.DTOs;
using System.Linq.Expressions;
using Microsoft.Data.SqlClient;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ErpOnlineOrderDbContext _context;
        private readonly IMapper _mapper;

        public OrderRepository(ErpOnlineOrderDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private IQueryable<OrderDTO> ProjectToOrderDto(IQueryable<Order> query)
        {
            return query.ProjectTo<OrderDTO>(_mapper.ConfigurationProvider);
        }

        private IQueryable<Order> GetBaseQuery()
        {
            return _context.Orders.AsNoTracking();
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await GetBaseQuery()
                .Include(o => o.Customer).ThenInclude(c => c!.User)
                .Include(o => o.Order_Details).ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order?> GetByIdForStatusCheckAsync(int id)
        {
            return await GetBaseQuery()
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order?> GetByIdForCopyAsync(int id)
        {
            return await GetBaseQuery()
                .Include(o => o.Order_Details)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order?> GetByIdForApprovalAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.Order_Details)
                .FirstOrDefaultAsync(o => o.Id == id && !o.Is_deleted);
        }

        public IQueryable<Order?> GetByOrderIdAsync(int id)
        {
            return GetBaseQuery()
                .Where(o => o.Id == id);
        }

        public async Task<IEnumerable<OrderDTO>> GetAllAsync()
        {
            var query = GetBaseQuery()
                .OrderByDescending(o => o.Order_date);
            return await ProjectToOrderDto(query).ToListAsync();
        }

        public async Task<int> CountAsync(Expression<Func<Order, bool>>? predicate = null)
        {
            var query = _context.Orders.AsNoTracking().Where(o => !o.Is_deleted);
            
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            
            return await query.CountAsync();
        }

        public async Task<PagedResult<Order>> GetPagedOrdersAsync(OrderFilterRequest request, IEnumerable<int>? customerIds = null)
        {
            var query = GetBaseQuery()
                .Include(o => o.Customer)
                .Include(o => o.Order_Details).ThenInclude(od => od.Product)
                .AsQueryable();

            if (customerIds != null)
            {
                var ids = customerIds.ToList();
                if (ids.Count > 0)
                    query = query.Where(o => ids.Contains(o.Customer_id));
                else
                    return new PagedResult<Order> { Items = new List<Order>(), Page = request.Page, PageSize = request.PageSize, TotalCount = 0 };
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(o => o.Order_status == request.Status);
            }

            if (request.DateFrom.HasValue)
            {
                query = query.Where(o => o.Order_date >= request.DateFrom.Value);
            }

            if (request.DateTo.HasValue)
            {
                var toDate = request.DateTo.Value.Date.AddDays(1);
                query = query.Where(o => o.Order_date < toDate);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.Trim().ToLowerInvariant();
                query = query.Where(o =>
                    (o.Order_code != null && o.Order_code.ToLower().Contains(search)) ||
                    (o.Customer != null && o.Customer.Full_name != null && o.Customer.Full_name.ToLower().Contains(search))
                );
            }

            query = query.OrderByDescending(o => o.Order_date);
            return await query.ToPagedListAsync(request);
        }

        public async Task<PagedResult<OrderDTO>> GetPagedOrdersDTOAsync(OrderFilterRequest request, IEnumerable<int>? customerIds = null)
        {
            var parameters = new List<SqlParameter>();
            string filterSql = "WHERE o.Parent_order_id IS NULL AND o.Is_deleted = 0";

            if (customerIds != null)
            {
                var idsList = customerIds.ToList();
                if (idsList.Count > 0)
                {
                    var ids = string.Join(",", idsList);
                    filterSql += $" AND o.Customer_id IN ({ids})"; 
                }
                else
                {
                    return new PagedResult<OrderDTO> { Items = new List<OrderDTO>(), Page = request.Page, PageSize = request.PageSize, TotalCount = 0 };
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                filterSql += " AND o.Order_status = @status";
                parameters.Add(new SqlParameter("@status", request.Status));
            }

            if (request.DateFrom.HasValue)
            {
                filterSql += " AND o.Order_date >= @dateFrom";
                parameters.Add(new SqlParameter("@dateFrom", request.DateFrom.Value));
            }

            if (request.DateTo.HasValue)
            {
                var toDate = request.DateTo.Value.Date.AddDays(1);
                filterSql += " AND o.Order_date < @dateTo";
                parameters.Add(new SqlParameter("@dateTo", toDate));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = $"%{request.SearchTerm.Trim().ToLowerInvariant()}%";
                filterSql += " AND (LOWER(o.Order_code) LIKE @search OR LOWER(c.Full_name) LIKE @search)";
                parameters.Add(new SqlParameter("@search", search));
            }

            int skip = (request.Page - 1) * request.PageSize;
            parameters.Add(new SqlParameter("@skip", skip));
            parameters.Add(new SqlParameter("@take", request.PageSize));

            string sql = $@"
                WITH OrderTree AS (
                    SELECT 
                        o.Id, o.Parent_order_id, o.Order_date,
                        0 AS IndentLevel,
                        o.Order_date AS RootOrderDate,
                        CAST(RIGHT('0000000000' + CAST(o.Id AS VARCHAR(10)), 10) AS VARCHAR(MAX)) AS SortPath
                    FROM Orders o
                    LEFT JOIN Customers c ON o.Customer_id = c.Id
                    {filterSql}

                    UNION ALL

                    SELECT 
                        child.Id, child.Parent_order_id, child.Order_date,
                        ot.IndentLevel + 1,
                        ot.RootOrderDate,
                        ot.SortPath + '-' + CAST(RIGHT('0000000000' + CAST(child.Id AS VARCHAR(10)), 10) AS VARCHAR(MAX))
                    FROM Orders child
                    INNER JOIN OrderTree ot ON child.Parent_order_id = ot.Id
                    WHERE child.Is_deleted = 0
                )
                SELECT Id, IndentLevel, COUNT(1) OVER() AS TotalCount 
                FROM OrderTree
                ORDER BY RootOrderDate DESC, SortPath ASC
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
                return new PagedResult<OrderDTO> { Items = new List<OrderDTO>(), Page = request.Page, PageSize = request.PageSize, TotalCount = 0 };

            var query = GetBaseQuery().Where(o => idList.Contains(o.Id));
            var dtosList = await ProjectToOrderDto(query).ToListAsync();

            var sortedDtos = new List<OrderDTO>();
            foreach (var id in idList)
            {
                var dto = dtosList.FirstOrDefault(d => d.Id == id);
                if (dto != null)
                {
                    dto.IndentLevel = indentDict[id];
                    sortedDtos.Add(dto);
                }
            }

            return new PagedResult<OrderDTO>
            {
                Items = sortedDtos,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<IEnumerable<Order>> GetByCustomerIdsAsync(IEnumerable<int> customerIds)
        {
            var ids = customerIds.ToList();
            if (ids.Count == 0) return new List<Order>();
            return await GetBaseQuery()
                .Where(o => ids.Contains(o.Customer_id))
                .Include(o => o.Customer)
                .Include(o => o.Order_Details).ThenInclude(od => od.Product)
                .OrderByDescending(o => o.Order_date)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderDTO>> GetByCustomerIdsDTOAsync(IEnumerable<int> customerIds)
        {
            var ids = customerIds.ToList();
            if (ids.Count == 0) return new List<OrderDTO>();
            var query = GetBaseQuery()
                .Where(o => ids.Contains(o.Customer_id))
                .OrderByDescending(o => o.Order_date);
            return await ProjectToOrderDto(query).ToListAsync();
        }

        public async Task<Order?> GetByCodeAsync(string code)
        {
            return await GetBaseQuery()
                .FirstOrDefaultAsync(o => o.Order_code == code);
        }

        public async Task AddAsync(Order order)
        {
            order.Created_at = DateTime.Now;
            order.Updated_at = DateTime.Now;
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Order order)
        {
            order.Updated_at = DateTime.Now;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.Is_deleted = true;
                order.Updated_at = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<OrderDTO?> GetOrderDetailsDtoAsync(int id)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.Id == id && !o.Is_deleted)
                .ProjectTo<OrderDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }
    }
}