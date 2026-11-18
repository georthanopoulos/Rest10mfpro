using Microsoft.EntityFrameworkCore;

namespace SchoolApp.Core;

public static class QueryablePaginationExtensions
{
    /// <summary>
    /// Counts the records, normalizes the page values and returns the requested page.
    /// The query must be ordered first, otherwise the pages are not stable.
    /// </summary>
    public static async Task<PaginatedResult<T>> ToPaginatedResultAsync<T>(
        this IOrderedQueryable<T> query, int pageNumber, int pageSize)
    {
        int totalRecords = await query.CountAsync();

        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = PaginationDefaults.DefaultPageSize;
        if (pageSize > PaginationDefaults.MaxPageSize) pageSize = PaginationDefaults.MaxPageSize;

        int skip = (pageNumber - 1) * pageSize;   // pages are 1-based, the offset in the DB starts from 0.

        List<T> data = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResult<T>(data, totalRecords, pageNumber, pageSize);
    }
}