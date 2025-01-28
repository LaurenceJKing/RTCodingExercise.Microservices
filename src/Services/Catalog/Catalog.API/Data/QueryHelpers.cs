using Catalog.API.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Linq.Expressions;

namespace Catalog.API.Data
{
    public static class QueryHelpers
    {
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> items, string sortBy, string sortOrder)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                return items;
            }

            var entityType = typeof(T);

            var property = entityType.GetProperty(sortBy) ?? throw new ArgumentException($"'{sortBy}' is not a property of {entityType.Name}");

            var parameter = Expression.Parameter(entityType, "x");

            var propertyAccess = Expression.MakeMemberAccess(parameter, property);

            var orderByExpression = Expression.Lambda(propertyAccess, parameter);

            // Use the appropriate OrderBy/OrderByDescending method
            var methodName = sortOrder == "asc" ? "OrderBy" : "OrderByDescending";
            var method = typeof(Queryable).GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(entityType, property.PropertyType);

            return method.Invoke(null, new object[] { items, orderByExpression }) as IQueryable<T>;
        }
        public static async Task<PaginatedList<T>> ToPagedListAsync<T>(this IQueryable<T> items, int pageNumber, int pageSize)
        {
            var totalItems = await items.CountAsync();
            var page = await items.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

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
