namespace SharedLibrary.Common.ResponseModel
{
    public class PagedResult<T> : Result
    {
        public IReadOnlyList<T> Items { get; }
        public int TotalCount { get; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        private PagedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize, bool isSuccess, Error error)
            : base(isSuccess, error)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        private static PagedResult<T> Success(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
        {
            return new PagedResult<T>(
                items.ToList(),
                totalCount,
                pageNumber,
                pageSize,
                true,
                Error.None
            );
        }

        private new static PagedResult<T> Failure(Error error)
        {
            return new PagedResult<T>(
                [],
                0,
                0,
                0,
                false,
                error
            );
        }

        public static PagedResult<T> Create(IEnumerable<T> source, int pageNumber, int pageSize)
        {
            try
            {
                var enumerable = source as T[] ?? source.ToArray();
                var count = enumerable.Count();
                var items = enumerable.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                return Success(items, count, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                return Failure(Error.FromException(ex));
            }
        }

        public static Task<PagedResult<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            try
            {
                var count = source.Count(); 
                var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                return Task.FromResult(Success(items, count, pageNumber, pageSize));
            }
            catch (Exception ex)
            {
                return Task.FromResult(Failure(Error.FromException(ex)));
            }
        }
    }
}
