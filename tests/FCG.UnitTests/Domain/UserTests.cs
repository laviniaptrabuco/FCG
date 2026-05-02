using FCG.Domain.Entities;
using FCG.Domain.Enums;
using FCG.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace FCG.UnitTests.Domain;

public class UserTests
{
    // ── Constructor ──────────────────────────────────────────────

    [Fact]
    public void Constructor_WithValidData_ShouldCreateUser()
    {
        // Arrange & Act
        var user = new User("João Silva", "joao@email.com", "hashedpassword", UserRole.User);

        // Assert
        user.Id.Should().NotBeEmpty();
        user.Name.Should().Be("João Silva");
        user.Email.Should().Be("joao@email.com");
        user.PasswordHash.Should().Be("hashedpassword");
        user.Role.Should().Be(UserRole.User);
        user.IsActive.Should().BeTrue();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null!)]
    public void Constructor_WithEmptyName_ShouldThrowDomainException(string? name)
    {
        // Act
        var act = () => new User(name!, "email@test.com", "hash");

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("*Name*");
    }

    [Fact]
    public void Constructor_WithNameTooShort_ShouldThrowDomainException()
    {
        var act = () => new User("A", "email@test.com", "hash");
        act.Should().Throw<DomainException>().WithMessage("*2*");
    }

    [Theory]
    [InlineData("invalidemail")]
    [InlineData("missing@")]
    [InlineData("@nodomain.com")]
    [InlineData("")]
    public void Constructor_WithInvalidEmail_ShouldThrowDomainException(string email)
    {
        var act = () => new User("Valid Name", email, "hash");
        act.Should().Throw<DomainException>().WithMessage("*email*", because: "email validation should fail");
    }

    [Fact]
    public void Constructor_EmailShouldBeStoredLowerCase()
    {
        var user = new User("Test User", "TEST@EMAIL.COM", "hash");
        user.Email.Should().Be("test@email.com");
    }

    // ── Update ───────────────────────────────────────────────────

    [Fact]
    public void Update_WithValidData_ShouldChangeNameAndEmail()
    {
        var user = new User("Old Name", "old@email.com", "hash");
        user.Update("New Name", "new@email.com");

        user.Name.Should().Be("New Name");
        user.Email.Should().Be("new@email.com");
    }

    // ── Activate / Deactivate ────────────────────────────────────

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var user = new User("Test", "test@test.com", "hash");
        user.Deactivate();
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveTrue()
    {
        var user = new User("Test", "test@test.com", "hash");
        user.Deactivate();
        user.Activate();
        user.IsActive.Should().BeTrue();
    }

    // ── ChangePassword ───────────────────────────────────────────

    [Fact]
    public void ChangePassword_WithValidHash_ShouldUpdatePassword()
    {
        var user = new User("Test", "test@test.com", "oldhash");
        user.ChangePassword("newhash");
        user.PasswordHash.Should().Be("newhash");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null!)]
    public void ChangePassword_WithEmptyHash_ShouldThrowDomainException(string? hash)
    {
        var user = new User("Test", "test@test.com", "hash");
        var act = () => user.ChangePassword(hash!);
        act.Should().Throw<DomainException>();
    }

    // ── AddGame / RemoveGame ─────────────────────────────────────

    [Fact]
    public void AddGame_NewGame_ShouldAddToLibrary()
    {
        var user = new User("Test", "test@test.com", "hash");
        var game = new Game("Minecraft", "Sandbox game", "Sandbox", 99.90m);

        user.AddGame(game);

        user.Games.Should().HaveCount(1);
        user.Games.First().GameId.Should().Be(game.Id);
    }

    [Fact]
    public void AddGame_DuplicateGame_ShouldThrowDomainException()
    {
        var user = new User("Test", "test@test.com", "hash");
        var game = new Game("Minecraft", "Sandbox game", "Sandbox", 99.90m);

        user.AddGame(game);
        var act = () => user.AddGame(game);

        act.Should().Throw<DomainException>()
           .WithMessage("*already*");
    }

    [Fact]
    public void RemoveGame_ExistingGame_ShouldRemoveFromLibrary()
    {
        var user = new User("Test", "test@test.com", "hash");
        var game = new Game("Minecraft", "Sandbox game", "Sandbox", 99.90m);
        user.AddGame(game);

        user.RemoveGame(game.Id);

        user.Games.Should().BeEmpty();
    }

    [Fact]
    public void RemoveGame_NonExistentGame_ShouldThrowDomainException()
    {
        var user = new User("Test", "test@test.com", "hash");
        var act = () => user.RemoveGame(Guid.NewGuid());
        act.Should().Throw<DomainException>();
    }

    // ── Role ─────────────────────────────────────────────────────

    [Fact]
    public void Constructor_DefaultRole_ShouldBeUser()
    {
        var user = new User("Test", "test@test.com", "hash");
        user.Role.Should().Be(UserRole.User);
    }

    [Fact]
    public void Constructor_WithAdminRole_ShouldBeAdmin()
    {
        var user = new User("Admin", "admin@test.com", "hash", UserRole.Admin);
        user.Role.Should().Be(UserRole.Admin);
    }
}
