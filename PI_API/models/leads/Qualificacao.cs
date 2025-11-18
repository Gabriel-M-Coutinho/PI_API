namespace PI_API.models.leads;

public class Qualificacao
{
    public string _id { get; set; }
    public string Descricao { get; set; }

    public Qualificacao(string id, string descricao)
    {
        _id = id;
        Descricao = descricao;
    }
}
