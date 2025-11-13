namespace PI_API.models.leads;

public class Estabelecimento
{
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
    public string CnaeSecundario { get; set; }
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

    public Estabelecimento(string cnpjBase, string cnpjOrdem, string cnpjDV, string matrizFilial, string nomeFantasia, string situacaoCadastral, DateTime? dataSituacaoCadastral, string motivoSituacaoCadastral, string cidadeExterior, string pais, DateTime? dataInicioAtividade, string cnaePrincipal, string cnaeSecundario, string tipoLogradouro, string logradouro, string numero, string complemento, string bairro, string cEP, string uF, string municipio, string ddd1, string telefone1, string ddd2, string telefone2, string dddFAX, string fAX, string correioEletronico, string situacaoEspecial, DateTime? dataSituacaoEspecial)
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
}
