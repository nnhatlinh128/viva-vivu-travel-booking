using Microsoft.EntityFrameworkCore;

namespace ToursAndTravelsManagement.Common;

public class PaginatedList<T> : List<T>
{
    public int PageIndex { get; private set; }
    public int TotalPages { get; private set; }

    public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        AddRange(items);
    }

    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;

    // ================== SYNC ==================
    public static PaginatedList<T> Create(
        IQueryable<T> source, int pageIndex, int pageSize)
    {
        var count = source.Count();
        var items = source
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }

    // ================== ASYNC ==================
    public static async Task<PaginatedList<T>> CreateAsync(
        IQueryable<T> source, int pageIndex, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }
}
