using FCG.Application.DTOs;
using FCG.Application.Services;
using FCG.Domain.Entities;
using FCG.Domain.Exceptions;
using FCG.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FCG.UnitTests.Application;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IGameRepository> _gameRepoMock = new();
    private readonly Mock<IPasswordHasher> _hasherMock = new();
    private readonly Mock<ILogger<UserService>> _loggerMock = new();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(
            _userRepoMock.Object,
            _gameRepoMock.Object,
            _hasherMock.Object,
            _loggerMock.Object);
    }

    // ── GetById ──────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_WithExistingUser_ShouldReturnUser()
    {
        var user = new User("Test User", "test@test.com", "hash");
        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);

        var result = await _sut.GetByIdAsync(user.Id);

        result.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentUser_ShouldThrowNotFoundException()
    {
        _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((User?)null);

        var act = () => _sut.GetByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── Update ───────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldReturnUpdatedUser()
    {
        var user = new User("Old Name", "old@test.com", "hash");
        var request = new UpdateUserRequest("New Name", "new@test.com");

        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email, default)).ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>(), default)).Returns(Task.CompletedTask);

        var result = await _sut.UpdateAsync(user.Id, request);

        result.Name.Should().Be("New Name");
        result.Email.Should().Be("new@test.com");
    }

    [Fact]
    public async Task UpdateAsync_WithEmailAlreadyUsedByOtherUser_ShouldThrow()
    {
        var user = new User("User A", "usera@test.com", "hash");
        var otherUser = new User("User B", "taken@test.com", "hash");
        var request = new UpdateUserRequest("User A", "taken@test.com");

        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email, default)).ReturnsAsync(otherUser);

        var act = () => _sut.UpdateAsync(user.Id, request);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*already in use*");
    }

    // ── AddGameToLibrary ─────────────────────────────────────────

    [Fact]
    public async Task AddGameToLibraryAsync_WithValidIds_ShouldAddGame()
    {
        var user = new User("Test", "test@test.com", "hash");
        var game = new Game("Minecraft", "desc", "Sandbox", 99m);

        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _gameRepoMock.Setup(r => r.GetByIdAsync(game.Id, default)).ReturnsAsync(game);
        _userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>(), default)).Returns(Task.CompletedTask);

        await _sut.AddGameToLibraryAsync(user.Id, game.Id);

        user.Games.Should().HaveCount(1);
        _userRepoMock.Verify(r => r.UpdateAsync(user, default), Times.Once);
    }

    [Fact]
    public async Task AddGameToLibraryAsync_WithInactiveGame_ShouldThrow()
    {
        var user = new User("Test", "test@test.com", "hash");
        var game = new Game("Inactive Game", "desc", "Action", 50m);
        game.Deactivate();

        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _gameRepoMock.Setup(r => r.GetByIdAsync(game.Id, default)).ReturnsAsync(game);

        var act = () => _sut.AddGameToLibraryAsync(user.Id, game.Id);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*not available*");
    }

    // ── ChangePassword ───────────────────────────────────────────

    [Fact]
    public async Task ChangePasswordAsync_WithCorrectCurrentPassword_ShouldSucceed()
    {
        var user = new User("Test", "test@test.com", "oldhash");
        var request = new ChangePasswordRequest("CurrentPass@1", "NewPass@123");

        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify(request.CurrentPassword, user.PasswordHash)).Returns(true);
        _hasherMock.Setup(h => h.Hash(request.NewPassword)).Returns("newhash");
        _userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>(), default)).Returns(Task.CompletedTask);

        await _sut.ChangePasswordAsync(user.Id, request);

        user.PasswordHash.Should().Be("newhash");
    }

    [Fact]
    public async Task ChangePasswordAsync_WithWrongCurrentPassword_ShouldThrow()
    {
        var user = new User("Test", "test@test.com", "oldhash");
        var request = new ChangePasswordRequest("WrongPass@1", "NewPass@123");

        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify(request.CurrentPassword, user.PasswordHash)).Returns(false);

        var act = () => _sut.ChangePasswordAsync(user.Id, request);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*incorrect*");
    }

    // ── Deactivate ───────────────────────────────────────────────

    [Fact]
    public async Task DeactivateAsync_WithExistingUser_ShouldDeactivate()
    {
        var user = new User("Test", "test@test.com", "hash");
        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>(), default)).Returns(Task.CompletedTask);

        await _sut.DeactivateAsync(user.Id);

        user.IsActive.Should().BeFalse();
    }
}
