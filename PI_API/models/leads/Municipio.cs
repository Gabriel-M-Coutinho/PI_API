namespace PI_API.models.leads;

public class Municipio
{
    public string _id { get; set; }
    public string Descricao { get; set; }

    public Municipio(string id, string descricao)
    {
        _id = id;
        Descricao = descricao;
    }
}
