using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PI_API.models.leads;

public class Simples
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string _id { get; set; }
    public string CnpjBase { get; set; }
    public string OpcaoDoSimples { get; set; }
    public DateTime? DataOpcaoDoSimples { get; set; }
    public DateTime? DataExclusaoDoSimples { get; set; }
    public string MEI { get; set; }
    public DateTime? DataOpcaoMEI { get; set; }
    public DateTime? DataExclusaoMEI { get; set; }

    public Simples(string cnpjBase, string opcaoDoSimples, DateTime? dataOpcaoDoSimples, DateTime? dataExclusaoDoSimples, string mEI, DateTime? dataOpcaoMEI, DateTime? dataExclusaoMEI)
    {
        CnpjBase = cnpjBase;
        OpcaoDoSimples = opcaoDoSimples;
        DataOpcaoDoSimples = dataOpcaoDoSimples;
        DataExclusaoDoSimples = dataExclusaoDoSimples;
        MEI = mEI;
        DataOpcaoMEI = dataOpcaoMEI;
        DataExclusaoMEI = dataExclusaoMEI;
    }
}
