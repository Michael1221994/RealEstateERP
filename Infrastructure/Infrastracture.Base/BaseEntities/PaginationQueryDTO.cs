using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastracture.Base.BaseEntities
{
    public class PagininatedRequest
    {
        public PaginaitonQueryDTO Param { get; set; } = new PaginaitonQueryDTO();
    }

    public class PaginaitonQueryDTO
    {
        private int _top = 1000;
        private const int MaxTop = 1000;

        public int Skip { get; set; } // Index

        public int Top
        {
            get => _top;
            set => _top = value > MaxTop ? MaxTop : value; // Logic: Cap at 1000 if value is higher
        }

        public bool RequireTotalCount { get; set; } // True means it return total count
        public int Count { get; set; } // Total count of data in the storage
        public OrderBy? OrderBy { get; set; }

        public PaginaitonQueryDTO()
        {
            this.Skip = 0;
            this.Top = 1000;
            this.RequireTotalCount = true;
            this.OrderBy = new OrderBy();
        }

        public PaginaitonQueryDTO(string orderBy, bool decending = false)
        {
            this.Skip = 0;
            this.Top = 1000;
            this.RequireTotalCount = true;
            this.OrderBy = new OrderBy() { Name = orderBy, OrderDecending = decending };
        }

        public bool IsValid()
        {
            if (this.Skip < 0 || this.Top < 0 || this.OrderBy == null)
            {
                return false;
            }
            if (string.IsNullOrEmpty(this.OrderBy.Name))
            {
                return false;
            }

            return true;
        }

        public void MapPaginationDetail(PaginaitonQueryDTO query)
        {
            this.Skip = query.Skip;
            this.Top = query.Top; // This will trigger the setter logic and cap at 1000
            this.RequireTotalCount = query.RequireTotalCount;
            this.Count = query.Count;
            this.OrderBy = query.OrderBy;
        }
    }

    public class OrderBy
    {
        public string? Name { get; set; }
        public bool OrderDecending { get; set; }
    }

    public class PaginatedList<T>
    {
        public List<T> Items { get; private set; } // The paginated data
        public int TotalCount { get; private set; } // Total number of items in the source
        public int Skip { get; private set; } // Number of skipped items
        public int Top { get; private set; } // Page size (Capped at 1000)

        public PaginatedList(List<T> items, int totalCount, int skip, int top)
        {
            Items = items;
            TotalCount = totalCount;
            Skip = skip;
            Top = top;
        }
    }

    public static class PaginationExtensions
    {
        public static async Task<PaginatedList<T>> ToPaginatedListAsync<T>(
            this IQueryable<T> source,
            PaginaitonQueryDTO? param)
        {
            // Use defaults if param is null
            param ??= new PaginaitonQueryDTO();

            // Safety check for Top
            if (param.Top <= 0)
            {
                param.Top = 10; // Default to 10 if 0 or negative provided
            }

            var skip = param.Skip;
            var top = param.Top;

            // Get total count only if requested (Performance optimization)
            int totalCount = 0;
            if (param.RequireTotalCount)
            {
                // Task.Run is used here to satisfy your existing signature, 
                // but in EF Core, use source.CountAsync() instead.
                totalCount = await Task.Run(() => source.Count());
            }

            // Retrieve the paginated data
            var items = await Task.Run(() => source.Skip(skip).Take(top).ToList());

            // Return the paginated list
            return new PaginatedList<T>(items, totalCount, skip, top);
        }
    }
}