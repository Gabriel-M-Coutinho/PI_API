using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using PI_API.models.leads;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace PI_API.dto
{
    public class LeadSearchDTO
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string CnpjBase { get; set; }
        public string? CnpjCompleto { get; set; } 
        public string CnpjOrdem { get; set; }
        public string CnpjDV { get; set; }
        public string MatrizFilial { get; set; }
        public string NomeFantasia { get; set; }
        public string SituacaoCadastral { get; set; }
        public DateTime? DataSituacaoCadastral { get; set; }
        public string MotivoSituacaoCadastral { get; set; }
        public string CidadeExterior { get; set; }
        public string Pais { get; set; }
        public DateTime? DataInicioAtividade { get; set; }
        public string CnaePrincipal { get; set; }

        // Lista nativa do .NET — funciona perfeitamente com JSON
        public List<string> CnaeSecundario { get; set; } = new();

        public string TipoLogradouro { get; set; }
        public string Logradouro { get; set; }
        public string Numero { get; set; }
        public string Complemento { get; set; }
        public string Bairro { get; set; }
        public string CEP { get; set; }
        public string UF { get; set; }
        public string Municipio { get; set; }
        public string Ddd1 { get; set; }
        public string Telefone1 { get; set; }
        public string Ddd2 { get; set; }
        public string Telefone2 { get; set; }
        public string DddFAX { get; set; }
        public string FAX { get; set; }
        public string CorreioEletronico { get; set; }
        public string SituacaoEspecial { get; set; }
        public DateTime? DataSituacaoEspecial { get; set; }
        public Cnae realCnaePrincipal { get; set; }
        public List<Cnae> realCnaeSecundario { get; set; }
        public List<Socio> Socios { get; set; }

        public LeadSearchDTO(
            string id,
            string cnpjBase,
            string cnpjOrdem,
            string cnpjDV,
            string matrizFilial,
            string nomeFantasia,
            string situacaoCadastral,
            DateTime? dataSituacaoCadastral,
            string motivoSituacaoCadastral,
            string cidadeExterior,
            string pais,
            DateTime? dataInicioAtividade,
            string cnaePrincipal,
            List<string> cnaeSecundario,
            string tipoLogradouro,
            string logradouro,
            string numero,
            string complemento,
            string bairro,
            string cep,
            string uf,
            string municipio,
            string ddd1,
            string telefone1,
            string ddd2,
            string telefone2,
            string dddFAX,
            string fax,
            string correioEletronico,
            string situacaoEspecial,
            DateTime? dataSituacaoEspecial,
            Cnae realCnaePrincipal,
            List<Cnae> realCnaeSecundario,
            List<Socio> socios
        )
        {
            _id = id;
            CnpjBase = cnpjBase;
            CnpjOrdem = cnpjOrdem;
            CnpjDV = cnpjDV;
            MatrizFilial = matrizFilial;
            NomeFantasia = nomeFantasia;
            SituacaoCadastral = situacaoCadastral;
            DataSituacaoCadastral = dataSituacaoCadastral;
            MotivoSituacaoCadastral = motivoSituacaoCadastral;
            CidadeExterior = cidadeExterior;
            Pais = pais;
            DataInicioAtividade = dataInicioAtividade;
            CnaePrincipal = cnaePrincipal;
            CnaeSecundario = cnaeSecundario ?? new List<string>();
            TipoLogradouro = tipoLogradouro;
            Logradouro = logradouro;
            Numero = numero;
            Complemento = complemento;
            Bairro = bairro;
            CEP = cep;
            UF = uf;
            Municipio = municipio;
            Ddd1 = ddd1;
            Telefone1 = telefone1;
            Ddd2 = ddd2;
            Telefone2 = telefone2;
            DddFAX = dddFAX;
            FAX = fax;
            CorreioEletronico = correioEletronico;
            SituacaoEspecial = situacaoEspecial;
            DataSituacaoEspecial = dataSituacaoEspecial;
            this.realCnaePrincipal = realCnaePrincipal;
            this.realCnaeSecundario = realCnaeSecundario ?? new List<Cnae>();
            Socios = socios ?? new List<Socio>();
        }

        public override string ToString()
        {
            string secCnaes = CnaeSecundario != null
                ? string.Join(", ", CnaeSecundario)
                : "null";

            string realSecCnaes = realCnaeSecundario != null
                ? string.Join(", ", realCnaeSecundario.Select(c => $"{c._id} - {c.Descricao}"))
                : "null";

            string sociosStr = Socios != null
                ? string.Join(", ", Socios.Select(s => s.ToString()))
                : "null";

            return
                $"Estabelecimento: {{ " +
                $"_id = {(_id ?? "null")}, " +
                $"CnpjBase = {CnpjBase}, " +
                $"CnpjOrdem = {CnpjOrdem}, " +
                $"CnpjDV = {CnpjDV}, " +
                $"MatrizFilial = {MatrizFilial}, " +
                $"NomeFantasia = {NomeFantasia}, " +
                $"SituacaoCadastral = {SituacaoCadastral}, " +
                $"DataSituacaoCadastral = {DataSituacaoCadastral}, " +
                $"MotivoSituacaoCadastral = {MotivoSituacaoCadastral}, " +
                $"CidadeExterior = {CidadeExterior}, " +
                $"Pais = {Pais}, " +
                $"DataInicioAtividade = {DataInicioAtividade}, " +
                $"CnaePrincipal = {CnaePrincipal}, " +
                $"realCnaePrincipal = {(realCnaePrincipal != null ? $"{realCnaePrincipal._id} - {realCnaePrincipal.Descricao}" : "null")}, " +
                $"CnaeSecundario = [{secCnaes}], " +
                $"realCnaeSecundario = [{realSecCnaes}], " +
                $"TipoLogradouro = {TipoLogradouro}, " +
                $"Logradouro = {Logradouro}, " +
                $"Numero = {Numero}, " +
                $"Complemento = {Complemento}, " +
                $"Bairro = {Bairro}, " +
                $"CEP = {CEP}, " +
                $"UF = {UF}, " +
                $"Municipio = {Municipio}, " +
                $"Ddd1 = {Ddd1}, " +
                $"Telefone1 = {Telefone1}, " +
                $"Ddd2 = {Ddd2}, " +
                $"Telefone2 = {Telefone2}, " +
                $"DddFAX = {DddFAX}, " +
                $"FAX = {FAX}, " +
                $"CorreioEletronico = {CorreioEletronico}, " +
                $"SituacaoEspecial = {SituacaoEspecial}, " +
                $"DataSituacaoEspecial = {DataSituacaoEspecial}, " +
                $"Socios = [{sociosStr}] }}";
        }

    } // valeu pela ajuda
}
