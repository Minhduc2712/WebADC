using Microsoft.EntityFrameworkCore;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Infrastructure.Extensions;
public static class QueryableExtensions
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 20;

    public static async Task<PagedResult<T>> ToPagedListAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var countTask = query.CountAsync(cancellationToken);
        var itemsTask = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        await Task.WhenAll(countTask, itemsTask);

        return new PagedResult<T>
        {
            Items = itemsTask.Result,
            Page = page,
            PageSize = pageSize,
            TotalCount = countTask.Result
        };
    }

    public static async Task<PagedResult<T>> ToPagedListAsync<T>(
        this IQueryable<T> query,
        BasePaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        return await query.ToPagedListAsync(
            request.Page,
            request.PageSize,
            cancellationToken);
    }
}

