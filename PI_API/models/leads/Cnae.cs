using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PI_API.models.leads;
public class Cnae
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string _id { get; set; }
    public string Descricao { get; set; }

    public Cnae(string id, string descricao)
    {
        _id = id;
        Descricao = descricao;
    }
}