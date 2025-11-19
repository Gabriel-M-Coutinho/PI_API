using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PI_API.models.leads;

public class Empresa
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string _id { get; set; }
    public string? CnpjBase { get; set; }
    public string? RazaoSocial { get; set; }
    public string? NaturezaJuridica { get; set; }
    public string? QualificacaoResponsavel { get; set; }
    public double CapitalSocial { get; set; }
    public string? PorteEmpresa { get; set; }
    public string? EnteFederativo { get; set; }

    public Empresa(string? cnpjBase, string? razaoSocial, string? naturezaJuridica, string? qualificacaoResponsavel, double capitalSocial, string? porteEmpresa, string? enteFederativo)
    {
        CnpjBase = cnpjBase;
        RazaoSocial = razaoSocial;
        NaturezaJuridica = naturezaJuridica;
        QualificacaoResponsavel = qualificacaoResponsavel;
        CapitalSocial = capitalSocial;
        PorteEmpresa = porteEmpresa;
        EnteFederativo = enteFederativo;
    }
}
