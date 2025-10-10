using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Repositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.WalletJournals.Queries;

public sealed record GetWalletJournalsQuery(Guid? BookingId, Guid? PaymentId) : IQuery<IEnumerable<WalletJournalResponse>>;

internal sealed class GetWalletJournalsQueryHandler : IQueryHandler<GetWalletJournalsQuery, IEnumerable<WalletJournalResponse>>
{
    private readonly IWalletJournalRepository _walletJournalRepository;
    private readonly IMapper _mapper;

    public GetWalletJournalsQueryHandler(IWalletJournalRepository walletJournalRepository, IMapper mapper)
    {
        _walletJournalRepository = walletJournalRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<WalletJournalResponse>>> Handle(GetWalletJournalsQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<WalletJournal> journals;

        if (request.BookingId.HasValue || request.PaymentId.HasValue)
        {
            Expression<Func<WalletJournal, bool>> predicate = journal =>
                (!request.BookingId.HasValue || journal.BookingId == request.BookingId) &&
                (!request.PaymentId.HasValue || journal.PaymentId == request.PaymentId);

            journals = await _walletJournalRepository.FindAsync(predicate, cancellationToken);
        }
        else
        {
            journals = await _walletJournalRepository.GetAllAsync(cancellationToken);
        }

        var response = _mapper.Map<IEnumerable<WalletJournalResponse>>(journals);
        return Result.Success(response);
    }
}
