using FCG.Domain.Entities;
using FCG.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace FCG.UnitTests.Domain;

public class GameTests
{
    // ── Constructor ──────────────────────────────────────────────

    [Fact]
    public void Constructor_WithValidData_ShouldCreateGame()
    {
        var game = new Game("Minecraft", "Sandbox open-world game", "Sandbox", 99.90m);

        game.Id.Should().NotBeEmpty();
        game.Title.Should().Be("Minecraft");
        game.Genre.Should().Be("Sandbox");
        game.Price.Should().Be(99.90m);
        game.IsActive.Should().BeTrue();
        game.PromotionalPrice.Should().BeNull();
        game.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null!)]
    public void Constructor_WithEmptyTitle_ShouldThrowDomainException(string? title)
    {
        var act = () => new Game(title!, "desc", "Genre", 10m);
        act.Should().Throw<DomainException>().WithMessage("*Title*");
    }

    [Fact]
    public void Constructor_WithNegativePrice_ShouldThrowDomainException()
    {
        var act = () => new Game("Valid Title", "desc", "Genre", -1m);
        act.Should().Throw<DomainException>().WithMessage("*negative*");
    }

    [Fact]
    public void Constructor_WithZeroPrice_ShouldBeAllowed()
    {
        // Free games are valid
        var act = () => new Game("Free Game", "desc", "Genre", 0m);
        act.Should().NotThrow();
    }

    // ── ApplyPromotion ───────────────────────────────────────────

    [Fact]
    public void ApplyPromotion_WithValidPrice_ShouldSetPromotionalPrice()
    {
        var game = new Game("Test Game", "desc", "Action", 100m);
        game.ApplyPromotion(50m);

        game.PromotionalPrice.Should().Be(50m);
        game.CurrentPrice.Should().Be(50m);
    }

    [Fact]
    public void ApplyPromotion_WithPriceHigherThanOriginal_ShouldThrow()
    {
        var game = new Game("Test Game", "desc", "Action", 100m);
        var act = () => game.ApplyPromotion(150m);
        act.Should().Throw<DomainException>().WithMessage("*less than*");
    }

    [Fact]
    public void ApplyPromotion_WithEqualPrice_ShouldThrow()
    {
        var game = new Game("Test Game", "desc", "Action", 100m);
        var act = () => game.ApplyPromotion(100m);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ApplyPromotion_WithZeroPrice_ShouldThrow()
    {
        var game = new Game("Test Game", "desc", "Action", 100m);
        var act = () => game.ApplyPromotion(0m);
        act.Should().Throw<DomainException>().WithMessage("*greater than zero*");
    }

    // ── RemovePromotion ──────────────────────────────────────────

    [Fact]
    public void RemovePromotion_ShouldClearPromotionalPrice()
    {
        var game = new Game("Test Game", "desc", "Action", 100m);
        game.ApplyPromotion(60m);
        game.RemovePromotion();

        game.PromotionalPrice.Should().BeNull();
        game.CurrentPrice.Should().Be(100m);
    }

    // ── CurrentPrice ─────────────────────────────────────────────

    [Fact]
    public void CurrentPrice_WithoutPromotion_ShouldReturnOriginalPrice()
    {
        var game = new Game("Test", "desc", "RPG", 79.90m);
        game.CurrentPrice.Should().Be(79.90m);
    }

    [Fact]
    public void CurrentPrice_WithPromotion_ShouldReturnPromotionalPrice()
    {
        var game = new Game("Test", "desc", "RPG", 79.90m);
        game.ApplyPromotion(39.90m);
        game.CurrentPrice.Should().Be(39.90m);
    }

    // ── Activate / Deactivate ────────────────────────────────────

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var game = new Game("Test", "desc", "Action", 50m);
        game.Deactivate();
        game.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveTrue()
    {
        var game = new Game("Test", "desc", "Action", 50m);
        game.Deactivate();
        game.Activate();
        game.IsActive.Should().BeTrue();
    }

    // ── Update ───────────────────────────────────────────────────

    [Fact]
    public void Update_WithValidData_ShouldChangeProperties()
    {
        var game = new Game("Old Title", "old desc", "Action", 50m);
        game.Update("New Title", "new desc", "RPG", 75m);

        game.Title.Should().Be("New Title");
        game.Description.Should().Be("new desc");
        game.Genre.Should().Be("RPG");
        game.Price.Should().Be(75m);
    }
}
