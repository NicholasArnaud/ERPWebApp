using ERPWebApp.Models.Common;
using System.Linq.Expressions;

namespace ERPWebApp.Data.Extensions
{
    public static class Query
    {
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition,
            Expression<Func<T, bool>> predicate)
        {
            return condition ? query.Where(predicate) : query;
        }
        public static IQueryable<T> SmartPaging<T>(this IQueryable<T> query, int? start = 0, int? pageSize = 10)
        {
            if (start != null && start >= 0 && pageSize != null && pageSize > 0)
            {
                query = query.Skip(((int)start));
            }

            if (pageSize != null)
            {
                query = query.Take((int)pageSize);
            }
            return query;
        }

        public static IQueryable<T> SmartSearch<T>(this IQueryable<T> query, List<string> searchColumns, string searchValue)
        {
            if (!string.IsNullOrEmpty(searchValue) && searchColumns != null && searchColumns.Count != 0)
            {
                query = query.Where(BuildSearchExpression<T>(searchColumns, searchValue));
            }
            return query;
        }

        public static IQueryable<T> SmartFilter<T>(this IQueryable<T> query, List<QueryFilter> filters)
        {
            if (filters != null && filters.Count != 0)
            {
                foreach (var filter in filters)
                {
                    if (filter.Column != null && filter.Column != "" && filter.Value != null && filter.Value != "")
                    {
                        query = query.Where(BuildFilterExpression<T>(filter.Column, filter.Value));
                    }
                }
            }

            return query;
        }

        public static IQueryable<T> SmartSort<T>(this IQueryable<T> query, string sortBy, bool isDescending)
        {
            // Apply sorting
            if (!string.IsNullOrEmpty(sortBy))
            {
                query = isDescending ? query.OrderByDescending(BuildSortExpression<T>(sortBy))
                    : query.OrderBy(BuildSortExpression<T>(sortBy));
            }
            return query;
        }


        private static Expression<Func<T, bool>> BuildSearchExpression<T>(List<string> searchColumns, string searchKey)
        {
            var parameter = Expression.Parameter(typeof(T), typeof(T).Name);

            Expression SearchExpression = null;

            foreach (var column in searchColumns)
            {
                Expression propertyAccess = parameter;
                // Split the column into parts to handle nested navigation properties
                var propertyNames = column.Split('.');

                foreach (var propertyName in propertyNames)
                {
                    propertyAccess = Expression.PropertyOrField(propertyAccess, propertyName);
                }

                // Convert the property access to string for case-insensitive comparison
                var toStringMethod = typeof(object).GetMethod("ToString");
                var toStringExpression = Expression.Call(propertyAccess, toStringMethod);
                var containsMethod = typeof(string).GetMethod("Contains", [typeof(string)]);
                var constant = Expression.Constant(searchKey, typeof(string));
                var containsExpression = Expression.Call(toStringExpression, containsMethod, constant);

                SearchExpression = SearchExpression == null ? containsExpression : Expression.Or(SearchExpression, containsExpression);
            }

            return Expression.Lambda<Func<T, bool>>(SearchExpression, parameter);
        }

        private static Expression<Func<T, bool>> BuildFilterExpression<T>(string column, string filterValue)
        {
            var parameter = Expression.Parameter(typeof(T), "entity");
            Expression propertyAccess = parameter;

            // Split the column into parts to handle nested navigation properties
            var propertyNames = column.Split('.');

            foreach (var propertyName in propertyNames)
            {
                propertyAccess = Expression.PropertyOrField(propertyAccess, propertyName);
            }

            var constant = Expression.Constant(filterValue, propertyAccess.Type);
            var equalExpression = Expression.Equal(propertyAccess, constant);

            return Expression.Lambda<Func<T, bool>>(equalExpression, parameter);
        }

        private static Expression<Func<T, object>> BuildSortExpression<T>(string sortBy)
        {
            var parameter = Expression.Parameter(typeof(T), "entity");
            Expression propertyAccess = parameter;

            // Split the sortBy into parts to handle nested navigation properties
            var propertyNames = sortBy.Split('.');

            foreach (var propertyName in propertyNames)
            {
                propertyAccess = Expression.PropertyOrField(propertyAccess, propertyName);
            }

            var conversion = Expression.Convert(propertyAccess, typeof(object));

            return Expression.Lambda<Func<T, object>>(conversion, parameter);
        }
    }
}
