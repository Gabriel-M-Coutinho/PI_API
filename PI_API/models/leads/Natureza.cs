namespace PI_API.models.leads;

public class Natureza
{
    public string Codigo { get; set; }
    public string Descricao { get; set; }

    public Natureza(string codigo, string descricao)
    {
        Codigo = codigo;
        Descricao = descricao;
    }
}
