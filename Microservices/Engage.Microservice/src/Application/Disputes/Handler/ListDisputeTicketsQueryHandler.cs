using Application.Abstractions.Messaging;
using Application.Disputes.Dtos;
using Application.Disputes.Query;
using Domain.IRepositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.Disputes.Handler;

public sealed class ListDisputeTicketsQueryHandler
    : IQueryHandler<ListDisputeTicketsQuery, IReadOnlyList<DisputeTicketDto>>
{
    private readonly IDisputeTicketRepository _disputeTicketRepository;
    private readonly IMapper _mapper;

    public ListDisputeTicketsQueryHandler(IDisputeTicketRepository disputeTicketRepository, IMapper mapper)
    {
        _disputeTicketRepository = disputeTicketRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<DisputeTicketDto>>> Handle(ListDisputeTicketsQuery request, CancellationToken cancellationToken)
    {
        var tickets = await _disputeTicketRepository.GetByStatusAsync(request.Status, cancellationToken);
        var dtos = tickets
            .Select(ticket => _mapper.Map<DisputeTicketDto>(ticket))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<DisputeTicketDto>>(dtos);
    }
}

