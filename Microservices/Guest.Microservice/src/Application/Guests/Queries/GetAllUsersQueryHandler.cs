using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharedLibrary.Common.ResponseModel;
using Application.Abstractions.Messaging;
using Domain.Repositories;
using MapsterMapper;
using MediatR;

namespace Application.Guests.Queries
{
    public sealed record GetAllGuestsQuery : IQuery<IEnumerable<GetGuestResponse>>;

    internal sealed class GetAllGuestsQueryHandler : IQueryHandler<GetAllGuestsQuery, IEnumerable<GetGuestResponse>>
    {
        private readonly IGuestRepository _guestRepository;
        private readonly IMapper _mapper;

        public GetAllGuestsQueryHandler(IGuestRepository guestRepository, IMapper mapper)
        {
            _guestRepository = guestRepository;
            _mapper = mapper;
        }


        public async Task<Result<IEnumerable<GetGuestResponse>>> Handle(GetAllGuestsQuery request,
            CancellationToken cancellationToken)
        {
            var users = await _guestRepository.GetAllAsync(cancellationToken);
            var userResponses = _mapper.Map<IEnumerable<GetGuestResponse>>(users);
            return Result.Success(userResponses);
        }
    }
}