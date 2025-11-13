namespace PI_API.models.leads;

public class Pais
{
    public string Codigo { get; set; }
    public string Descricao { get; set; }

    public Pais(string codigo, string descricao)
    {
        Codigo = codigo;
        Descricao = descricao;
    }
}
