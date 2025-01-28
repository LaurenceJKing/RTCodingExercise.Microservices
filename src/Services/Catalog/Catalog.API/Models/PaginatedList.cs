namespace Catalog.API.Models
{
    public class PaginatedList<T>
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 20;
        public int TotalPages { get; init; }
        public int TotalItems { get; init; }

        public IReadOnlyCollection<T> Items { get; init; }
    }

    public static class PaginationHelpers
    {
        public static async Task<PaginatedList<T>> ToPagedListAsync<T>(this IQueryable<T> items, int pageNumber, int pageSize)
        {
            var totalItems = await items.CountAsync();
            var page = await items.Skip((pageNumber -1)*pageSize).Take(pageSize).ToListAsync();

            return new PaginatedList<T>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                Items = page,
            };
        }
    }
}
