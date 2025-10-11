using Application.Wallets.Commands;
using FluentValidation;

namespace Application.Wallets.Validators;

public sealed class UpdateWalletCommandValidator : AbstractValidator<UpdateWalletCommand>
{
    public UpdateWalletCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Status)
            .IsInEnum();
    }
}
