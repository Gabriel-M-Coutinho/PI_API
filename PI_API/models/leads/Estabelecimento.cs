using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PI_API.models.leads;

public class Estabelecimento
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string _id { get; set; }
    public string CnpjBase { get; set; }
    public string CnpjOrdem { get; set; }
    public string CnpjDV { get; set; }
    public string MatrizFilial { get; set; }
    public string NomeFantasia { get; set; }
    public string SituacaoCadastral { get; set; }
    public DateTime? DataSituacaoCadastral { get; set; }
    public string MotivoSituacaoCadastral { get; set; }
    public string CidadeExterior { get; set; }
    public string Pais { get; set; }
    public DateTime? DataInicioAtividade { get; set; }
    public string CnaePrincipal { get; set; }
    public BsonArray CnaeSecundario { get; set; }
    public string TipoLogradouro { get; set; }
    public string Logradouro { get; set; }
    public string Numero { get; set; }
    public string Complemento { get; set; }
    public string Bairro { get; set; }
    public string CEP { get; set; }
    public string UF { get; set; }
    public string Municipio { get; set; }
    public string Ddd1 { get; set; }
    public string Telefone1 { get; set; }
    public string Ddd2 { get; set; }
    public string Telefone2 { get; set; }
    public string DddFAX { get; set; }
    public string FAX { get; set; }
    public string CorreioEletronico { get; set; }
    public string SituacaoEspecial { get; set; }
    public DateTime? DataSituacaoEspecial { get; set; }

    public Estabelecimento(string cnpjBase, string cnpjOrdem, string cnpjDV, string matrizFilial, string nomeFantasia, string situacaoCadastral, DateTime? dataSituacaoCadastral, string motivoSituacaoCadastral, string cidadeExterior, string pais, DateTime? dataInicioAtividade, string cnaePrincipal, BsonArray cnaeSecundario, string tipoLogradouro, string logradouro, string numero, string complemento, string bairro, string cEP, string uF, string municipio, string ddd1, string telefone1, string ddd2, string telefone2, string dddFAX, string fAX, string correioEletronico, string situacaoEspecial, DateTime? dataSituacaoEspecial)
    {
        CnpjBase = cnpjBase;
        CnpjOrdem = cnpjOrdem;
        CnpjDV = cnpjDV;
        MatrizFilial = matrizFilial;
        NomeFantasia = nomeFantasia;
        SituacaoCadastral = situacaoCadastral;
        DataSituacaoCadastral = dataSituacaoCadastral;
        MotivoSituacaoCadastral = motivoSituacaoCadastral;
        CidadeExterior = cidadeExterior;
        Pais = pais;
        DataInicioAtividade = dataInicioAtividade;
        CnaePrincipal = cnaePrincipal;
        CnaeSecundario = cnaeSecundario;
        TipoLogradouro = tipoLogradouro;
        Logradouro = logradouro;
        Numero = numero;
        Complemento = complemento;
        Bairro = bairro;
        CEP = cEP;
        UF = uF;
        Municipio = municipio;
        Ddd1 = ddd1;
        Telefone1 = telefone1;
        Ddd2 = ddd2;
        Telefone2 = telefone2;
        DddFAX = dddFAX;
        FAX = fAX;
        CorreioEletronico = correioEletronico;
        SituacaoEspecial = situacaoEspecial;
        DataSituacaoEspecial = dataSituacaoEspecial;
    }
    public override string ToString()
    {
        return $@"
        Id: {_id}
        CnpjBase: {CnpjBase}
        CnpjOrdem: {CnpjOrdem}
        CnpjDV: {CnpjDV}
        MatrizFilial: {MatrizFilial}
        NomeFantasia: {NomeFantasia}
        SituacaoCadastral: {SituacaoCadastral}
        DataSituacaoCadastral: {DataSituacaoCadastral}
        MotivoSituacaoCadastral: {MotivoSituacaoCadastral}
        CidadeExterior: {CidadeExterior}
        Pais: {Pais}
        DataInicioAtividade: {DataInicioAtividade}
        CnaePrincipal: {CnaePrincipal}
        CnaeSecundario: {(CnaeSecundario == null ? "" : string.Join(", ", CnaeSecundario))}
        TipoLogradouro: {TipoLogradouro}
        Logradouro: {Logradouro}
        Numero: {Numero}
        Complemento: {Complemento}
        Bairro: {Bairro}
        CEP: {CEP}
        UF: {UF}
        Municipio: {Municipio}
        Ddd1: {Ddd1}
        Telefone1: {Telefone1}
        Ddd2: {Ddd2}
        Telefone2: {Telefone2}
        DddFAX: {DddFAX}
        FAX: {FAX}
        CorreioEletronico: {CorreioEletronico}
        SituacaoEspecial: {SituacaoEspecial}
        DataSituacaoEspecial: {DataSituacaoEspecial}
        ".Trim();
    }

}
