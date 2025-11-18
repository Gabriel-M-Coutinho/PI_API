namespace PI_API.models.leads;

public class Pais
{
    public string _id { get; set; }
    public string Descricao { get; set; }

    public Pais(string id, string descricao)
    {
        _id = id;
        Descricao = descricao;
    }
}
