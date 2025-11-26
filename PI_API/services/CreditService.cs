using MongoDB.Driver;
using LeadSearch.Models;
using PI_API.models; 

namespace PI_API.services;

public class CreditService
{
    private readonly IMongoCollection<ApplicationUser> _userCollection;
    private readonly ContextMongodb _context;

    public CreditService(ContextMongodb context)
    {
        _context = context;
        _userCollection = context.User;
    }

    public async Task AddCredits(string userId, int credits)
    {
        try
        {
            var filter = Builders<ApplicationUser>.Filter.Eq("Id", userId);
            var user = await _userCollection.Find(filter).FirstOrDefaultAsync();

            if (user == null)
            {
                throw new Exception($"Usuário não encontrado com ID: {userId}");
            }

            user.Credits += credits;
            user.UpdatedAt = DateTime.UtcNow;

            var replaceResult = await _userCollection.ReplaceOneAsync(filter, user);

            if (replaceResult.ModifiedCount == 0)
            {
                throw new Exception("Falha ao atualizar créditos do usuário");
            }

            if (replaceResult.ModifiedCount > 0)
            {
                Console.WriteLine($"Créditos adicionados: {credits} para usuário {userId}");
                Console.WriteLine($"Novo total: {user.Credits}");
            }

            Console.WriteLine($"Créditos adicionados (método alternativo): {credits} para usuário {userId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro: {ex}");
        }
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