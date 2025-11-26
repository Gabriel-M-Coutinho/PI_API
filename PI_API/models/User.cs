using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PI_API.models;

public class User
{
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? CpfCnpj { get; set; }
        public string? Tipo { get; set; }
        public long Credits { get; set; } = 0;
        public DateTime CreatedAt { get; set; } 
        public DateTime UpdatedAt { get; set; }
        public bool Active { get; set; } = true;

        public User(){}
        public User(string Email, string Password, string FullName, string CpfCnpj)
        {
                this.Email = Email;
                this.Password = Password;
                this.FullName = FullName;
                this.CpfCnpj = CpfCnpj;
                CreatedAt = DateTime.Now;
                Active = false;
        }
}