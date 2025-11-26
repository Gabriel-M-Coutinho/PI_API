using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PI_API.models;

public class Order
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
  
    public string SessionId { get; set; } // ID da sessão do Stripe
    public CreditPlan Plan { get; set; }
    public string PackageName { get; set; }
    public int Credits { get; set; }
    public double Price { get; set; }
    public bool IsPaid { get; set; } = false;
    public string Status { get; set; } = "pending"; // pending, paid, failed, canceled
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }
    public string CustomerEmail { get; set; }
    public string CustomerId { get; set; } // ID do usuário se estiver logado
}