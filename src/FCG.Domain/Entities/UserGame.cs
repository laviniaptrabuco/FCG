namespace FCG.Domain.Entities;

public class UserGame
{
    public Guid UserId { get; private set; }
    public Guid GameId { get; private set; }
    public DateTime AcquiredAt { get; private set; }

    public User? User { get; private set; }
    public Game? Game { get; private set; }

    protected UserGame() { }

    public UserGame(Guid userId, Guid gameId)
    {
        UserId = userId;
        GameId = gameId;
        AcquiredAt = DateTime.UtcNow;
    }
}
