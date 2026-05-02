using FCG.Domain.Enums;
using FCG.Domain.Exceptions;

namespace FCG.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<UserGame> _games = new();
    public IReadOnlyCollection<UserGame> Games => _games.AsReadOnly();

    protected User() { }

    public User(string name, string email, string passwordHash, UserRole role = UserRole.User)
    {
        ValidateName(name);
        ValidateEmail(email);

        Id = Guid.NewGuid();
        Name = name;
        Email = email.ToLowerInvariant();
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public void Update(string name, string email)
    {
        ValidateName(name);
        ValidateEmail(email);
        Name = name;
        Email = email.ToLowerInvariant();
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainException("Password hash cannot be empty.");
        PasswordHash = newPasswordHash;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    public void AddGame(Game game)
    {
        if (_games.Any(g => g.GameId == game.Id))
            throw new DomainException($"Game '{game.Title}' is already in the library.");
        _games.Add(new UserGame(Id, game.Id));
    }

    public void RemoveGame(Guid gameId)
    {
        var userGame = _games.FirstOrDefault(g => g.GameId == gameId)
            ?? throw new DomainException("Game not found in library.");
        _games.Remove(userGame);
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name cannot be empty.");
        if (name.Length < 2 || name.Length > 100)
            throw new DomainException("Name must be between 2 and 100 characters.");
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email cannot be empty.");
        if (!System.Text.RegularExpressions.Regex.IsMatch(email,
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new DomainException("Invalid email format.");
    }
}
