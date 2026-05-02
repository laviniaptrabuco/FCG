using FCG.Domain.Entities;
using FCG.Domain.Enums;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace FCG.Infrastructure.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    static MongoDbContext()
    {
        var guidSerializer = new GuidSerializer(BsonType.String);
        var decimalSerializer = new DecimalSerializer(BsonType.String);
        var nullableDecimalSerializer = new NullableSerializer<decimal>(decimalSerializer);

        if (!BsonClassMap.IsClassMapRegistered(typeof(Game)))
        {
            BsonClassMap.RegisterClassMap<Game>(cm =>
            {
                cm.MapIdMember(g => g.Id).SetSerializer(guidSerializer);
                cm.MapProperty(g => g.Title);
                cm.MapProperty(g => g.Description);
                cm.MapProperty(g => g.Genre);
                cm.MapProperty(g => g.Price).SetSerializer(decimalSerializer);
                cm.MapProperty(g => g.PromotionalPrice).SetSerializer(nullableDecimalSerializer);
                cm.MapProperty(g => g.CreatedAt);
                cm.MapProperty(g => g.IsActive);
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(UserGame)))
        {
            BsonClassMap.RegisterClassMap<UserGame>(cm =>
            {
                cm.MapProperty(ug => ug.UserId).SetSerializer(guidSerializer);
                cm.MapProperty(ug => ug.GameId).SetSerializer(guidSerializer);
                cm.MapProperty(ug => ug.AcquiredAt);
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(User)))
        {
            BsonClassMap.RegisterClassMap<User>(cm =>
            {
                cm.MapIdMember(u => u.Id).SetSerializer(guidSerializer);
                cm.MapProperty(u => u.Name);
                cm.MapProperty(u => u.Email);
                cm.MapProperty(u => u.PasswordHash);
                cm.MapProperty(u => u.Role)
                    .SetSerializer(new EnumSerializer<UserRole>(BsonType.String));
                cm.MapProperty(u => u.CreatedAt);
                cm.MapProperty(u => u.IsActive);
                cm.MapField("_games").SetElementName("games");
                cm.SetIgnoreExtraElements(true);
            });
        }
    }

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        var databaseName = configuration["MongoDB:DatabaseName"] ?? "FiapCloudGames";
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<Game> Games => _database.GetCollection<Game>("games");
}
