using FCG.Domain.Entities;
using FCG.Domain.Interfaces;
using FCG.Infrastructure.Data;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace FCG.Infrastructure.Repositories.Mongo;

public class MongoGameRepository : IGameRepository
{
    private readonly MongoDbContext _context;

    public MongoGameRepository(MongoDbContext context) => _context = context;

    public async Task<Game?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Games.Find(g => g.Id == id).FirstOrDefaultAsync(ct);

    public async Task<IEnumerable<Game>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Games.Find(_ => true).ToListAsync(ct);

    public async Task<IEnumerable<Game>> GetByGenreAsync(string genre, CancellationToken ct = default)
    {
        var filter = Builders<Game>.Filter.Regex(
            g => g.Genre,
            new BsonRegularExpression($"^{Regex.Escape(genre)}$", "i"));
        return await _context.Games.Find(filter).ToListAsync(ct);
    }

    public async Task<Game> AddAsync(Game game, CancellationToken ct = default)
    {
        await _context.Games.InsertOneAsync(game, cancellationToken: ct);
        return game;
    }

    public async Task UpdateAsync(Game game, CancellationToken ct = default) =>
        await _context.Games.ReplaceOneAsync(g => g.Id == game.Id, game, cancellationToken: ct);

    public async Task<bool> ExistsByTitleAsync(string title, CancellationToken ct = default)
    {
        var filter = Builders<Game>.Filter.Regex(
            g => g.Title,
            new BsonRegularExpression($"^{Regex.Escape(title)}$", "i"));
        return await _context.Games.CountDocumentsAsync(filter, cancellationToken: ct) > 0;
    }
}
