using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SharedLibrary.Common.ResponseModel;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Repositories;
using MapsterMapper;
using SharedLibrary.Common;

namespace Application.Guests.Commands
{
    public sealed record CreateGuestCommand(
        string Fullname,
        string Email
    ) : ICommand;
    internal sealed class CreateGuestCommandHandler : ICommandHandler<CreateGuestCommand>
    {
        private readonly IGuestRepository _guestRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateGuestCommandHandler(IGuestRepository guestRepository, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _guestRepository = guestRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result> Handle(CreateGuestCommand command, CancellationToken cancellationToken)
        {
            await _guestRepository.AddAsync(_mapper.Map<Guest>(command), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        
    }
}