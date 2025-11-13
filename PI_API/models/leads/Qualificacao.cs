namespace PI_API.models.leads;

public class Qualificacao
{
    public string Codigo { get; set; }
    public string Descricao { get; set; }

    public Qualificacao(string codigo, string descricao)
    {
        Codigo = codigo;
        Descricao = descricao;
    }
}
