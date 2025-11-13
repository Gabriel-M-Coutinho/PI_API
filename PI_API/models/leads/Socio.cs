namespace PI_API.models.leads;

public class Socio
{
    public string CnpjBase { get; set; }
    public string IdentificadoSocio { get; set; }
    public string NomeSocio { get; set; }
    public string CnpjCpf { get; set; }
    public string QualificaoSocio { get; set; }
    public DateTime? DataEntradaSociedade { get; set; }
    public string Pais { get; set; }
    public string RepresentanteLegal { get; set; }
    public string NomeRepresentante { get; set; }
    public string QualificacaoResponsavel { get; set; }
    public string FaixaEtaria { get; set; }

    public Socio(string cnpjBase, string identificadoSocio, string nomeSocio, string cnpjCpf, string qualificaoSocio, DateTime? dataEntradaSociedade, string pais, string representanteLegal, string nomeRepresentante, string qualificacaoResponsavel, string faixaEtaria)
    {
        CnpjBase = cnpjBase;
        IdentificadoSocio = identificadoSocio;
        NomeSocio = nomeSocio;
        CnpjCpf = cnpjCpf;
        QualificaoSocio = qualificaoSocio;
        DataEntradaSociedade = dataEntradaSociedade;
        Pais = pais;
        RepresentanteLegal = representanteLegal;
        NomeRepresentante = nomeRepresentante;
        QualificacaoResponsavel = qualificacaoResponsavel;
        FaixaEtaria = faixaEtaria;
    }
}
