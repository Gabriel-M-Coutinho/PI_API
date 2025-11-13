namespace PI_API.models.leads;

public class Motivo
{
    public string Codigo { get; set; }
    public string Descricao { get; set; }

    public Motivo(string codigo, string descricao)
    {
        Codigo = codigo;
        Descricao = descricao;
    }
}
