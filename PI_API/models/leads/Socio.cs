using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PI_API.models.leads;

public class Socio
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string _id { get; set; }
    public string CnpjBase { get; set; }
    public string IdentificadorSocio { get; set; }
    public string NomeSocio { get; set; }
    public string CnpjCpf { get; set; }
    public string QualificacaoSocio { get; set; }
    public DateTime? DataEntradaSociedade { get; set; }
    public string Pais { get; set; }
    public string RepresentanteLegal { get; set; }
    public string NomeRepresentante { get; set; }
    public string QualificacaoResponsavel { get; set; }
    public string FaixaEtaria { get; set; }

    public Socio(string cnpjBase, string identificadorSocio, string nomeSocio, string cnpjCpf, string qualificacaoSocio, DateTime? dataEntradaSociedade, string pais, string representanteLegal, string nomeRepresentante, string qualificacaoResponsavel, string faixaEtaria)
    {
        CnpjBase = cnpjBase;
        IdentificadorSocio = identificadorSocio;
        NomeSocio = nomeSocio;
        CnpjCpf = cnpjCpf;
        QualificacaoSocio = qualificacaoSocio;
        DataEntradaSociedade = dataEntradaSociedade;
        Pais = pais;
        RepresentanteLegal = representanteLegal;
        NomeRepresentante = nomeRepresentante;
        QualificacaoResponsavel = qualificacaoResponsavel;
        FaixaEtaria = faixaEtaria;
    }
}
