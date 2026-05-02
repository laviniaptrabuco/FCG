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

public class GameServiceTests
{
    private readonly Mock<IGameRepository> _gameRepoMock = new();
    private readonly Mock<ILogger<GameService>> _loggerMock = new();
    private readonly GameService _sut;

    public GameServiceTests()
    {
        _sut = new GameService(_gameRepoMock.Object, _loggerMock.Object);
    }

    // ── Create ───────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldReturnGameResponse()
    {
        // Arrange
        var request = new CreateGameRequest("Minecraft", "Sandbox game", "Sandbox", 99.90m);

        _gameRepoMock.Setup(r => r.ExistsByTitleAsync(request.Title, default))
            .ReturnsAsync(false);
        _gameRepoMock.Setup(r => r.AddAsync(It.IsAny<Game>(), default))
            .ReturnsAsync((Game g, CancellationToken _) => g);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(request.Title);
        result.Price.Should().Be(request.Price);
        result.IsActive.Should().BeTrue();
        _gameRepoMock.Verify(r => r.AddAsync(It.IsAny<Game>(), default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateTitle_ShouldThrowDomainException()
    {
        // Arrange
        var request = new CreateGameRequest("Existing Game", "desc", "Action", 50m);
        _gameRepoMock.Setup(r => r.ExistsByTitleAsync(request.Title, default))
            .ReturnsAsync(true);

        // Act
        var act = () => _sut.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*already exists*");
    }

    // ── GetById ──────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnGame()
    {
        // Arrange
        var game = new Game("Test Game", "desc", "RPG", 59.90m);
        _gameRepoMock.Setup(r => r.GetByIdAsync(game.Id, default))
            .ReturnsAsync(game);

        // Act
        var result = await _sut.GetByIdAsync(game.Id);

        // Assert
        result.Id.Should().Be(game.Id);
        result.Title.Should().Be(game.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        // Arrange
        _gameRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Game?)null);

        // Act
        var act = () => _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── ApplyPromotion ───────────────────────────────────────────

    [Fact]
    public async Task ApplyPromotionAsync_WithValidPrice_ShouldReturnUpdatedGame()
    {
        // Arrange
        var game = new Game("Test", "desc", "Action", 100m);
        var request = new ApplyPromotionRequest(50m);

        _gameRepoMock.Setup(r => r.GetByIdAsync(game.Id, default)).ReturnsAsync(game);
        _gameRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Game>(), default)).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ApplyPromotionAsync(game.Id, request);

        // Assert
        result.PromotionalPrice.Should().Be(50m);
        result.CurrentPrice.Should().Be(50m);
    }

    [Fact]
    public async Task ApplyPromotionAsync_WithPriceHigherThanOriginal_ShouldThrow()
    {
        // Arrange
        var game = new Game("Test", "desc", "Action", 100m);
        _gameRepoMock.Setup(r => r.GetByIdAsync(game.Id, default)).ReturnsAsync(game);

        // Act
        var act = () => _sut.ApplyPromotionAsync(game.Id, new ApplyPromotionRequest(200m));

        // Assert
        await act.Should().ThrowAsync<DomainException>();
    }

    // ── Deactivate ───────────────────────────────────────────────

    [Fact]
    public async Task DeactivateAsync_WithExistingGame_ShouldDeactivate()
    {
        // Arrange
        var game = new Game("Test", "desc", "Action", 100m);
        _gameRepoMock.Setup(r => r.GetByIdAsync(game.Id, default)).ReturnsAsync(game);
        _gameRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Game>(), default)).Returns(Task.CompletedTask);

        // Act
        await _sut.DeactivateAsync(game.Id);

        // Assert
        game.IsActive.Should().BeFalse();
        _gameRepoMock.Verify(r => r.UpdateAsync(game, default), Times.Once);
    }

    // ── GetAll ───────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllGames()
    {
        // Arrange
        var games = new List<Game>
        {
            new("Game 1", "desc", "Action", 50m),
            new("Game 2", "desc", "RPG", 70m),
            new("Game 3", "desc", "Strategy", 30m)
        };
        _gameRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(games);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
    }
}
