using System;
using System.Linq;
using Application.Abstractions.Interface;
using Application.Abstractions.Messaging;
using Application.Authentication.Command;
using Application.Features;
using Domain.Entities;
using Domain.Enums;
using Domain.IRepositories;
using Microsoft.AspNetCore.Identity;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.Authentication.Handler;

public sealed class LoginWithGoogleCommandHandler :
    ICommandHandler<LoginWithGoogleCommand, AuthDto.LoginResultDto>,
    ICommandHandler<LoginWithGoogleViaIdTokenCommand, AuthDto.LoginResultDto>
{
    private const string GoogleProvider = "google";

    private readonly IAuthenticationRepository _authRepository;
    private readonly IExternalLoginRepository _externalLoginRepository;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IRefreshTokenStore _refreshTokenStore;
    private readonly IGoogleOAuthService _googleOAuthService;
    private readonly IGoogleOAuthStateStore _stateStore;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher<AppUser> _passwordHasher;

    public LoginWithGoogleCommandHandler(
        IAuthenticationRepository authRepository,
        IExternalLoginRepository externalLoginRepository,
        IJwtTokenGenerator jwt,
        IRefreshTokenStore refreshTokenStore,
        IGoogleOAuthService googleOAuthService,
        IGoogleOAuthStateStore stateStore,
        IUnitOfWork unitOfWork,
        IPasswordHasher<AppUser> passwordHasher)
    {
        _authRepository = authRepository;
        _externalLoginRepository = externalLoginRepository;
        _jwt = jwt;
        _refreshTokenStore = refreshTokenStore;
        _googleOAuthService = googleOAuthService;
        _stateStore = stateStore;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<AuthDto.LoginResultDto>> Handle(LoginWithGoogleCommand request, CancellationToken ct)
    {
        var stateData = await _stateStore.RedeemAsync(request.State, ct);
        if (stateData is null)
        {
            throw new UnauthorizedAccessException("Invalid or expired Google OAuth state.");
        }

        var redirectUri = stateData.RedirectUri;
        if (!string.IsNullOrWhiteSpace(request.RedirectUri) &&
            !string.Equals(request.RedirectUri, redirectUri, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Redirect URI does not match initial authorization request.");
        }

        var googleUser = await _googleOAuthService.ExchangeCodeAsync(
            request.Code,
            redirectUri,
            stateData.CodeVerifier,
            ct);

        var dto = await SignInAsync(googleUser, ct);
        return Result.Success(dto);
    }

    public async Task<Result<AuthDto.LoginResultDto>> Handle(LoginWithGoogleViaIdTokenCommand request, CancellationToken ct)
    {
        var googleUser = await _googleOAuthService.ValidateIdTokenAsync(request.IdToken, ct);
        var dto = await SignInAsync(googleUser, ct);
        return Result.Success(dto);
    }

    private async Task<AuthDto.LoginResultDto> SignInAsync(GoogleOAuthUser googleUser, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var googleSubject = googleUser.Subject;
        if (string.IsNullOrWhiteSpace(googleSubject))
        {
            throw new UnauthorizedAccessException("Google profile is missing subject identifier.");
        }

        var normalizedEmail = googleUser.Email.Trim().ToLowerInvariant();
        var mapping = await _externalLoginRepository.FindAsync(GoogleProvider, googleSubject, ct);
        AppUser user;

        if (mapping is not null)
        {
            user = mapping.User ?? await _authRepository.GetByIdAsync(mapping.UserId, ct)
                ?? throw new UnauthorizedAccessException("Linked account no longer exists.");

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("Account is inactive.");
            }

            ApplyGoogleProfile(user, googleUser, now);
            _authRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        else
        {
            user = await LinkOrCreateUserAsync(googleUser, normalizedEmail, now, ct);
        }

        var roleIds = user.Roles.Select(r => r.Id).ToArray();
        var roleDtos = user.Roles.Select(r => new RoleDto(r.Id, r.DisplayName)).ToArray();

        var accessToken = _jwt.CreateAccessToken(user, roleIds);
        var refreshToken = await _refreshTokenStore.IssueAsync(user.Id, ct);

        return new AuthDto.LoginResultDto(
            user.Id,
            user.Email!,
            user.FullName,
            roleDtos,
            new AuthDto.TokenPairDto(accessToken, refreshToken));
    }

    private async Task<AppUser> LinkOrCreateUserAsync(GoogleOAuthUser googleUser, string normalizedEmail, DateTime now, CancellationToken ct)
    {
        AppUser? user = null;
        if (googleUser.EmailVerified)
        {
            user = await _authRepository.FindByEmailAsync(normalizedEmail, ct);
        }
        else if (await _authRepository.ExistsByEmailAsync(normalizedEmail, ct))
        {
            throw new UnauthorizedAccessException("Google email is not verified for an existing account.");
        }

        if (user is null)
        {
            user = await CreateUserAsync(googleUser, normalizedEmail, now, ct);
        }
        else
        {
            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("Account is inactive.");
            }

            ApplyGoogleProfile(user, googleUser, now);
            _authRepository.Update(user);
        }

        var externalLogin = new ExternalLogin
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Provider = GoogleProvider,
            ProviderUserId = googleUser.Subject,
            CreatedAt = now,
            User = user
        };

        await _externalLoginRepository.AddAsync(externalLogin, ct);
        await _unitOfWork.SaveChangesAsync(ct);


        return user;
    }

    private async Task<AppUser> CreateUserAsync(GoogleOAuthUser googleUser, string normalizedEmail, DateTime now, CancellationToken ct)
    {
        var roleRepository = _unitOfWork.Repository<Role>();
        var customerRole = (await roleRepository.FindAsync(r => r.Id == RoleType.Customer.GetId(), ct)).FirstOrDefault();
        if (customerRole is null)
        {
            customerRole = RoleType.Customer.ToEntity();
            await roleRepository.AddAsync(customerRole, ct);
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            FullName = string.IsNullOrWhiteSpace(googleUser.FullName) ? normalizedEmail : googleUser.FullName!,
            AvatarUrl = googleUser.PictureUrl,
            IsActive = true,
            EmailConfirmed = googleUser.EmailVerified,
            SecurityStamp = Guid.NewGuid().ToString(),
            CreatedAt = now,
            UpdatedAt = now,
            LastLoginAt = now
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, Guid.NewGuid().ToString("N"));
        user.Roles.Add(customerRole);

        await _authRepository.AddAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return user;
    }

    private static void ApplyGoogleProfile(AppUser user, GoogleOAuthUser googleUser, DateTime now)
    {
        if (!user.EmailConfirmed && googleUser.EmailVerified)
        {
            user.EmailConfirmed = true;
        }

        if (string.IsNullOrWhiteSpace(user.FullName) && !string.IsNullOrWhiteSpace(googleUser.FullName))
        {
            user.FullName = googleUser.FullName!;
        }

        if (string.IsNullOrWhiteSpace(user.AvatarUrl) && !string.IsNullOrWhiteSpace(googleUser.PictureUrl))
        {
            user.AvatarUrl = googleUser.PictureUrl;
        }

        user.LastLoginAt = now;
        user.UpdatedAt = now;
    }
}









