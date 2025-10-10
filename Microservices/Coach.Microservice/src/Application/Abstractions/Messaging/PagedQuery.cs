using System;
using SharedLibrary.Common.ResponseModel;

namespace Application.Abstractions.Messaging;

/// <summary>
/// Base query type for paginated read operations. Normalizes supplied paging arguments.
/// </summary>
/// <typeparam name="TResponse">Response payload type contained inside the paged result.</typeparam>
public abstract record PagedQuery<TResponse> : IQuery<PagedResult<TResponse>>
{
    private const int DefaultPageNumber = 1;
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;

    protected PagedQuery(int pageNumber = DefaultPageNumber, int pageSize = DefaultPageSize)
    {
        PageNumber = pageNumber < 1 ? DefaultPageNumber : pageNumber;
        PageSize = NormalizePageSize(pageSize);
    }

    public int PageNumber { get; init; }

    public int PageSize { get; init; }

    private static int NormalizePageSize(int pageSize)
    {
        if (pageSize < 1)
        {
            return DefaultPageSize;
        }

        return Math.Min(pageSize, MaxPageSize);
    }
}
