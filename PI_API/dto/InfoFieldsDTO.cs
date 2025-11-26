using PI_API.models.leads;

namespace PI_API.dto
{
    public class InfoFieldsDTO
    {
        public List<Cnae> Cnaes { get; set; }
        public List<Municipio> Municipios { get; set; }

        public InfoFieldsDTO(List<Cnae> cnaes, List<Municipio> municipios)
        {
            Cnaes = cnaes;
            Municipios = municipios;
        }
    }
}
