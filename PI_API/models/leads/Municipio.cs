namespace PI_API.models.leads;

public class Municipio
{
    public string Codigo { get; set; }
    public string Descricao { get; set; }

    public Municipio(string codigo, string descricao)
    {
        Codigo = codigo;
        Descricao = descricao;
    }
}
