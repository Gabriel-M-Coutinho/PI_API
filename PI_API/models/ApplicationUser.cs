using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;
using PI_API.models;

namespace LeadSearch.Models
{
    
    [CollectionName("Users")]
    public class ApplicationUser : MongoIdentityUser
    {
        
        public string FullName { get; set; } = string.Empty;
        public string CpfCnpj { get; set; } = string.Empty;
        public string? Tipo { get; set; }
        public bool Active { get; set; } = true;
        public long Credits { get; set; } = 0;
        public List<string> CnpjsComprados { get; set; } = new List<string>();
        [BsonElement("role")]
        public ROLE Role { get; set; } = ROLE.STANDARD;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}