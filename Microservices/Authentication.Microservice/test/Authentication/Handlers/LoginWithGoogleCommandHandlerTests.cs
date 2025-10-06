using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Interface;
using Application.Authentication.Command;
using Application.Authentication.Handler;
using Application.Features;
using Domain.Entities;
using Domain.Enums;
using Domain.IRepositories;
using Microsoft.AspNetCore.Identity;
using FluentAssertions;
using Moq;
using SharedLibrary.Common;
using Xunit;

namespace test.Authentication.Handlers;

public class LoginWithGoogleCommandHandlerTests
{
    private readonly Mock<IAuthenticationRepository> _authRepository = new();
    private readonly Mock<IExternalLoginRepository> _externalLoginRepository = new();
    private readonly Mock<IJwtTokenGenerator> _jwt = new();
    private readonly Mock<IRefreshTokenStore> _refreshTokenStore = new();
    private readonly Mock<IGoogleOAuthService> _googleOAuthService = new();
    private readonly Mock<IGoogleOAuthStateStore> _stateStore = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IPasswordHasher<AppUser>> _passwordHasher = new();
    private readonly Mock<IRepository<Role>> _roleRepository = new();

    private void ResetMocks()
    {
        _authRepository.Reset();
        _externalLoginRepository.Reset();
        _jwt.Reset();
        _refreshTokenStore.Reset();
        _googleOAuthService.Reset();
        _stateStore.Reset();
        _unitOfWork.Reset();
        _passwordHasher.Reset();
        _roleRepository.Reset();
    }
    private LoginWithGoogleCommandHandler CreateHandler()
    {
        _jwt.Setup(x => x.CreateAccessToken(It.IsAny<AppUser>(), It.IsAny<IEnumerable<string>>()))
            .Returns("access-token");
        _refreshTokenStore.Setup(x => x.IssueAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("refresh-token");
        _passwordHasher.Setup(x => x.HashPassword(It.IsAny<AppUser>(), It.IsAny<string>()))
            .Returns("hashed");
        _unitOfWork.Setup(x => x.Repository<Role>()).Returns(_roleRepository.Object);
        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        return new LoginWithGoogleCommandHandler(
            _authRepository.Object,
            _externalLoginRepository.Object,
            _jwt.Object,
            _refreshTokenStore.Object,
            _googleOAuthService.Object,
            _stateStore.Object,
            _unitOfWork.Object,
            _passwordHasher.Object);
    }

    [Fact]
    public async Task Handle_ViaIdToken_WithExistingExternalLogin_ShouldReturnTokens()
    {
        ResetMocks();
        var userId = Guid.NewGuid();
        var user = new AppUser
        {
            Id = userId,
            Email = "user@example.com",
            FullName = "User",
            IsActive = true,
            EmailConfirmed = true,
            Roles = new List<Role> { RoleType.Customer.ToEntity() }
        };

        var externalLogin = new ExternalLogin
        {
            Id = Guid.NewGuid(),
            Provider = "google",
            ProviderUserId = "google-sub",
            UserId = userId,
            User = user
        };

        _externalLoginRepository
            .Setup(x => x.FindAsync("google", "google-sub", It.IsAny<CancellationToken>()))
            .ReturnsAsync(externalLogin);

        _googleOAuthService
            .Setup(x => x.ValidateIdTokenAsync("id-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleOAuthUser("google-sub", "user@example.com", true, "User", null));

        var handler = CreateHandler();
        var result = await handler.Handle(new LoginWithGoogleViaIdTokenCommand("id-token"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Tokens.AccessToken.Should().Be("access-token");
        result.Value.Tokens.RefreshToken.Should().Be("refresh-token");
        user.LastLoginAt.Should().NotBeNull();
        _externalLoginRepository.Verify(x => x.AddAsync(It.IsAny<ExternalLogin>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ViaIdToken_NoMapping_EmailExistsAndVerified_ShouldLink()
    {
        ResetMocks();
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = "member@example.com",
            FullName = string.Empty,
            IsActive = true,
            EmailConfirmed = false,
            Roles = new List<Role> { RoleType.Customer.ToEntity() }
        };

        _externalLoginRepository
            .Setup(x => x.FindAsync("google", "google-sub", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExternalLogin?)null);

        _authRepository
            .Setup(x => x.FindByEmailAsync("member@example.com", It.IsAny<CancellationToken>(), false))
            .ReturnsAsync(user);

        _googleOAuthService
            .Setup(x => x.ValidateIdTokenAsync("id-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleOAuthUser("google-sub", "member@example.com", true, "Member Name", "avatar"));

        var handler = CreateHandler();
        var result = await handler.Handle(new LoginWithGoogleViaIdTokenCommand("id-token"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.EmailConfirmed.Should().BeTrue();
        user.FullName.Should().Be("Member Name");
        _authRepository.Verify(x => x.Update(user), Times.Once);
        _externalLoginRepository.Verify(x => x.AddAsync(It.IsAny<ExternalLogin>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ViaIdToken_NoMapping_EmailExistsButNotVerified_ShouldThrow()
    {
        ResetMocks();
        _externalLoginRepository
            .Setup(x => x.FindAsync("google", "google-sub", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExternalLogin?)null);

        _authRepository
            .Setup(x => x.ExistsByEmailAsync("member@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _googleOAuthService
            .Setup(x => x.ValidateIdTokenAsync("id-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleOAuthUser("google-sub", "member@example.com", false, "Member Name", null));

        var handler = CreateHandler();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new LoginWithGoogleViaIdTokenCommand("id-token"), CancellationToken.None));

        _externalLoginRepository.Verify(x => x.AddAsync(It.IsAny<ExternalLogin>(), It.IsAny<CancellationToken>()), Times.Never);
        _authRepository.Verify(x => x.AddAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ViaIdToken_NoMapping_EmailMissing_ShouldCreateUser()
    {
        ResetMocks();
        _externalLoginRepository
            .Setup(x => x.FindAsync("google", "google-sub", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExternalLogin?)null);

        _authRepository
            .Setup(x => x.FindByEmailAsync("new@example.com", It.IsAny<CancellationToken>(), false))
            .ReturnsAsync((AppUser?)null);

        _googleOAuthService
            .Setup(x => x.ValidateIdTokenAsync("id-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleOAuthUser("google-sub", "new@example.com", true, "New User", null));

        _roleRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Role>());

        var handler = CreateHandler();
        var result = await handler.Handle(new LoginWithGoogleViaIdTokenCommand("id-token"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _authRepository.Verify(x => x.AddAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()), Times.Once);
        _externalLoginRepository.Verify(x => x.AddAsync(It.IsAny<ExternalLogin>(), It.IsAny<CancellationToken>()), Times.Once);
        _roleRepository.Verify(x => x.AddAsync(It.Is<Role>(r => r.DisplayName == RoleType.Customer.GetDisplayName()), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCode_InvalidState_ShouldThrow()
    {
        ResetMocks();
        _stateStore
            .Setup(x => x.RedeemAsync("bad-state", It.IsAny<CancellationToken>()))
            .ReturnsAsync((GoogleOAuthStateData?)null);

        var handler = CreateHandler();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new LoginWithGoogleCommand("code", "bad-state", null), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ViaIdToken_InactiveUser_ShouldThrow()
    {
        ResetMocks();
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            IsActive = false,
            EmailConfirmed = true,
            Roles = new List<Role> { RoleType.Customer.ToEntity() }
        };

        var external = new ExternalLogin
        {
            Id = Guid.NewGuid(),
            Provider = "google",
            ProviderUserId = "google-sub",
            UserId = user.Id,
            User = user
        };

        _externalLoginRepository
            .Setup(x => x.FindAsync("google", "google-sub", It.IsAny<CancellationToken>()))
            .ReturnsAsync(external);

        _googleOAuthService
            .Setup(x => x.ValidateIdTokenAsync("id-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleOAuthUser("google-sub", "user@example.com", true, "User", null));

        var handler = CreateHandler();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new LoginWithGoogleViaIdTokenCommand("id-token"), CancellationToken.None));
    }
}









