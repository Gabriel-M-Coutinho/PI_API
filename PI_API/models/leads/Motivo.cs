using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PI_API.models.leads;

public class Motivo
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string _id { get; set; }
    public string Descricao { get; set; }

    public Motivo(string id, string descricao)
    {
        _id = id;
        Descricao = descricao;
    }
}
