using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PI_API.models;

public class User
{
    
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;


        public string? Name { get; set; } = string.Empty;


        public string? Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        
        
        public string? FullName { get; set; }
        public string? CpfCnpj { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Boolean Active { get; set; }

        public User(){}
        public User(string Name, string Email, string Password, string FullName, string CpfCnpj)
        {
                this.Name = Name;
                this.Email = Email;
                this.Password = Password;
                this.FullName = FullName;
                this.CpfCnpj = CpfCnpj;
                CreatedAt = DateTime.Now;
                Active = false;
        }


}