using FCG.Application.DTOs;
using FCG.Application.Interfaces;
using FCG.Application.Services;
using FCG.Domain.Entities;
using FCG.Domain.Enums;
using FCG.Domain.Exceptions;
using FCG.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FCG.UnitTests.Application;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IPasswordHasher> _hasherMock = new();
    private readonly Mock<IJwtService> _jwtMock = new();
    private readonly Mock<ILogger<AuthService>> _loggerMock = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(
            _userRepoMock.Object,
            _hasherMock.Object,
            _jwtMock.Object,
            _loggerMock.Object);
    }

    // ── Login ────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var user = new User("Test User", "user@test.com", "hashedpwd", UserRole.User);
        var request = new LoginRequest("user@test.com", "Senha@123");
        var expectedToken = "jwt-token-here";
        var expiration = DateTime.UtcNow.AddHours(8);

        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email, default))
            .ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify(request.Password, user.PasswordHash))
            .Returns(true);
        _jwtMock.Setup(j => j.GenerateToken(user)).Returns(expectedToken);
        _jwtMock.Setup(j => j.GetExpiration()).Returns(expiration);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Token.Should().Be(expectedToken);
        result.Email.Should().Be(user.Email);
        result.Role.Should().Be("User");
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ShouldThrowDomainException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync((User?)null);

        // Act
        var act = () => _sut.LoginAsync(new LoginRequest("nope@test.com", "Senha@123"));

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Invalid*");
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldThrowDomainException()
    {
        // Arrange
        var user = new User("Test", "user@test.com", "hash");
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        // Act
        var act = () => _sut.LoginAsync(new LoginRequest("user@test.com", "wrongpwd"));

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Invalid*");
    }

    [Fact]
    public async Task LoginAsync_WithDeactivatedUser_ShouldThrowDomainException()
    {
        // Arrange
        var user = new User("Test", "user@test.com", "hash");
        user.Deactivate();

        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync(user);

        // Act
        var act = () => _sut.LoginAsync(new LoginRequest("user@test.com", "Senha@123"));

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*deactivated*");
    }

    // ── Register ─────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var request = new RegisterUserRequest("New User", "new@test.com", "Senha@123");

        _userRepoMock.Setup(r => r.ExistsByEmailAsync(request.Email, default))
            .ReturnsAsync(false);
        _hasherMock.Setup(h => h.Hash(request.Password)).Returns("hashedpwd");
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>(), default))
            .ReturnsAsync((User u, CancellationToken _) => u);

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.Email.Should().Be(request.Email.ToLower());
        result.Name.Should().Be(request.Name);
        result.Role.Should().Be("User");
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), default), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldThrowDomainException()
    {
        // Arrange
        var request = new RegisterUserRequest("User", "existing@test.com", "Senha@123");
        _userRepoMock.Setup(r => r.ExistsByEmailAsync(request.Email, default))
            .ReturnsAsync(true);

        // Act
        var act = () => _sut.RegisterAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*already*");
    }

    [Fact]
    public async Task RegisterAsync_WithWeakPassword_ShouldThrowDomainException()
    {
        // Arrange
        var request = new RegisterUserRequest("User", "test@test.com", "fraco");
        _userRepoMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync(false);

        // Act
        var act = () => _sut.RegisterAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
    }
}
