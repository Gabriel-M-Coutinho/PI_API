namespace PI_API.models.leads;

public class Empresa
{
    public string? CnpjBase { get; set; }
    public string? RazaoSocial { get; set; }
    public string? NaturezaJuridica { get; set; }
    public string? QualificacaoResponsavel { get; set; }
    public decimal CapitalSocial { get; set; }
    public string? PorteEmpresa { get; set; }
    public string? EnteFederativo { get; set; }

    public Empresa(string? cnpjBase, string? razaoSocial, string? naturezaJuridica, string? qualificacaoResponsavel, decimal capitalSocial, string? porteEmpresa, string? enteFederativo)
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
