using LeadSearch.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using PI_API.db;
using PI_API.models;

    namespace PI_API.services;

    public class UserService
    {
        private readonly IMongoCollection<ApplicationUser> _usersCollection;

        public UserService(IOptions<MongoDbSettings> mongoSettings)
        {
            var mongoClient = new MongoClient(mongoSettings.Value.ConnectionString);
            var database = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);

            _usersCollection = database.GetCollection<ApplicationUser>(mongoSettings.Value.UsersCollectionName);
        }

        public async Task<List<ApplicationUser>> GetAsync() =>
            await _usersCollection.Find(_ => true).ToListAsync();

        public async Task<ApplicationUser?> GetByIdAsync(string id)
        {
            return await _usersCollection.Find(u => u.Id.ToString() == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(ApplicationUser user)
        { 
            await _usersCollection.InsertOneAsync(user);
        }

        public async Task<ApplicationUser> GetByEmail(string  email) {
            return await _usersCollection.Find(u => u.Email == email).FirstOrDefaultAsync();    
        }

        public async Task UpdateAsync(string id, ApplicationUser user)
        {
            await _usersCollection.ReplaceOneAsync(u => u.Id.ToString() == id, user);
        }
            
        public async Task DeleteAsync(string id)
        {
            await _usersCollection.DeleteOneAsync(u => u.Id.ToString() == id);
        }
            
    }