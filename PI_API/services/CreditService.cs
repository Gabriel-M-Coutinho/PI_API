using MongoDB.Driver;
using LeadSearch.Models;
using PI_API.models; 

namespace PI_API.services;

public class CreditService
{
    private readonly IMongoCollection<ApplicationUser> _userCollection;

    public CreditService(ContextMongodb context)
    {

        _userCollection = context.User;
    }

    public async Task AddCredits(string userId, int credits)
    {
        var filter = Builders<ApplicationUser>.Filter.Eq(u => u.Id.ToString(), userId);
        var update = Builders<ApplicationUser>.Update
            .Inc(u => u.Credits, credits)
            .Set(u => u.UpdatedAt, DateTime.UtcNow);

        await _userCollection.UpdateOneAsync(filter, update);
    }

    public async Task<bool> UseCredits(string userId, int credits)
    {
        var user = await _userCollection
            .Find(u => u.Id.ToString() == userId)
            .FirstOrDefaultAsync();

        if (user == null || user.Credits < credits)
            return false;

        var filter = Builders<ApplicationUser>.Filter.Eq(u => u.Id.ToString(), userId);
        var update = Builders<ApplicationUser>.Update
            .Inc(u => u.Credits, -credits)
            .Set(u => u.UpdatedAt, DateTime.UtcNow);

        await _userCollection.UpdateOneAsync(filter, update);
        return true;
    }

    public async Task<long> GetUserCredits(string userId)
    {
        var user = await _userCollection
            .Find(u => u.Id.ToString() == userId)
            .Project(u => new { u.Credits })
            .FirstOrDefaultAsync();

        return user?.Credits ?? 0;
    }
}