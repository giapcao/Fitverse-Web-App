using SharedLibrary.Common.ResponseModel;

namespace Application.Abstractions.Messaging;

public abstract record PagedQuery<TResponse>(int PageNumber = 1, int PageSize = 10) : IQuery<PagedResult<TResponse>>
{
    public int PageNumber { get; init; } = PageNumber < 1 ? 1 : PageNumber;
    public int PageSize { get; init; } = PageSize switch
    {
        < 1 => 10,
        > 100 => 100,
        _ => PageSize
    };
}
