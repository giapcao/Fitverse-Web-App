
using System.Net;
using Application.Abstractions.Interface;
using Application.Abstractions.Messaging;
using Application.Authentication.Validator;
using Domain.Entities;
using Domain.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.Authentication.Handler;

public sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand, Unit>
{
    private readonly IAuthenticationRepository _authen;
    private readonly IEmailSender _email;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        IAuthenticationRepository authen,
        IEmailSender email,
        IJwtTokenGenerator jwt,
        IUnitOfWork unitOfWork)
    {
        _authen = authen;
        _email = email;
        _jwt = jwt;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(RegisterCommand request, CancellationToken ct)
    {
        if (await _authen.ExistsByEmailAsync(request.Email.Trim().ToLower(), ct))
        {
            return Result.Failure<Unit>(new Error("500","Email existed"));
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email.Trim().ToLower(),
            FullName = request.FullName,
            IsActive = true,
            EmailConfirmed = false,
            SecurityStamp = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt =  DateTime.UtcNow
        };

        var hasher = new PasswordHasher<AppUser>();
        user.PasswordHash = hasher.HashPassword(user, request.Password);

        await _authen.AddAsync(user, ct);

        var emailToken = _jwt.CreatePurposeToken(
            user.Id,
            "email_confirm",
            TimeSpan.FromMinutes(30));

        var link = $"{request.ConfirmBaseUrl}?token={WebUtility.UrlEncode(emailToken)}";

        await _email.SendAsync(
            user.Email!,
            "Xác thực email",
            $"Nhấp để xác thực: {link}",
            ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success(Unit.Value);
    }
}
