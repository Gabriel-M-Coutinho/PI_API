namespace PI_API.models.leads;
public class Cnae
{
    public string Codigo { get; set; }
    public string Descricao { get; set; }

    public Cnae(string codigo, string descricao)
    {
        Codigo = codigo;
        Descricao = descricao;
    }
}