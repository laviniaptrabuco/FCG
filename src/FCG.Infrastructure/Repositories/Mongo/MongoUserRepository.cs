using FCG.Domain.Entities;
using FCG.Domain.Interfaces;
using FCG.Infrastructure.Data;
using MongoDB.Driver;
using System.Reflection;

namespace FCG.Infrastructure.Repositories.Mongo;

public class MongoUserRepository : IUserRepository
{
    private readonly MongoDbContext _context;

    private static readonly PropertyInfo _gameProp =
        typeof(UserGame).GetProperty(nameof(UserGame.Game))!;

    public MongoUserRepository(MongoDbContext context) => _context = context;

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync(ct);
        if (user is not null) await PopulateGamesAsync(user, ct);
        return user;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.ToLowerInvariant();
        var user = await _context.Users.Find(u => u.Email == normalized).FirstOrDefaultAsync(ct);
        if (user is not null) await PopulateGamesAsync(user, ct);
        return user;
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Users.Find(_ => true).ToListAsync(ct);

    public async Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        await _context.Users.InsertOneAsync(user, cancellationToken: ct);
        return user;
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default) =>
        await _context.Users.ReplaceOneAsync(u => u.Id == user.Id, user, cancellationToken: ct);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.ToLowerInvariant();
        return await _context.Users.CountDocumentsAsync(
            u => u.Email == normalized, cancellationToken: ct) > 0;
    }

    private async Task PopulateGamesAsync(User user, CancellationToken ct)
    {
        var gameIds = user.Games.Select(ug => ug.GameId).ToList();
        if (gameIds.Count == 0) return;

        var filter = Builders<Game>.Filter.In(g => g.Id, gameIds);
        var games = await _context.Games.Find(filter).ToListAsync(ct);
        var gameMap = games.ToDictionary(g => g.Id);

        foreach (var userGame in user.Games)
        {
            if (gameMap.TryGetValue(userGame.GameId, out var game))
                _gameProp.SetValue(userGame, game);
        }
    }
}
