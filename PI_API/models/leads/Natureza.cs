using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PI_API.models.leads;

public class Natureza
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string _id { get; set; }
    public string Descricao { get; set; }

    public Natureza(string id, string descricao)
    {
        _id = id;
        Descricao = descricao;
    }
}
