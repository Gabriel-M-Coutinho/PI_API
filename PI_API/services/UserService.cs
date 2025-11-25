using LeadSearch.Models;
using Microsoft.AspNetCore.Http.HttpResults;
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
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(IOptions<MongoDbSettings> mongoSettings, UserManager<ApplicationUser> userManager)
    {
        var mongoClient = new MongoClient(mongoSettings.Value.ConnectionString);
        var database = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);

        _usersCollection = database.GetCollection<ApplicationUser>(mongoSettings.Value.UsersCollectionName);
        _userManager = userManager;
}

    public async Task<List<ApplicationUser>> GetAsync() => await _usersCollection.Find(_ => true).ToListAsync();

    public async Task<ApplicationUser?> GetByIdAsync(string id)
    {
        return await _userManager.FindByIdAsync(id);
    }

    public async Task<ApplicationUser?> GetByEmail(string email)
    {
        string normalized = _userManager.NormalizeEmail(email);
        return await _usersCollection.Find(u => u.NormalizedEmail == normalized).FirstOrDefaultAsync();
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationUser user)
    {
        return await _userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> DeleteAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found" });

        return await _userManager.DeleteAsync(user);
    }
    public async Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword)
    {
        return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
    }

    public async Task<IList<string>> GetRolesAsync(ApplicationUser user)
    {
        return await _userManager.GetRolesAsync(user);
    }
}