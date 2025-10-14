using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PI_API.db;
using PI_API.models;

namespace PI_API.services;

public class UserService
{
    private readonly IMongoCollection<User> _usersCollection;

    public UserService(IOptions<MongoDbSettings> mongoSettings)
    {
        var mongoClient = new MongoClient(mongoSettings.Value.ConnectionString);
        var database = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
        _usersCollection = database.GetCollection<User>(mongoSettings.Value.UsersCollectionName);
    }

    public async Task<List<User>> GetAsync() =>
        await _usersCollection.Find(_ => true).ToListAsync();


    public async Task<User?> GetByIdAsync(string id) =>
        await _usersCollection.Find(u => u.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(User user) =>
        await _usersCollection.InsertOneAsync(user);


    public async Task UpdateAsync(string id, User user) =>
        await _usersCollection.ReplaceOneAsync(u => u.Id == id, user);


    public async Task DeleteAsync(string id) =>
        await _usersCollection.DeleteOneAsync(u => u.Id == id);
}