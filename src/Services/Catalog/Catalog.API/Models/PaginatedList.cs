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
}
