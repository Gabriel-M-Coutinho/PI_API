namespace PI_API.models.leads;

public class Motivo
{
    public string _id { get; set; }
    public string Descricao { get; set; }

    public Motivo(string id, string descricao)
    {
        _id = id;
        Descricao = descricao;
    }
}
