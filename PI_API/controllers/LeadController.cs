using LeadSearch.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDbGenericRepository;
using PI_API.dto;
using PI_API.models;
using PI_API.models.leads;
using PI_API.services;
using System.Text.Json;

namespace PI_API.controllers
{
    

    [Route("api/[controller]")]
    [ApiController]
    public class LeadController : ControllerBase
    {
        private readonly ContextMongodb _context;
        public LeadController(ContextMongodb context)
        {
            _context = context;
        }

        private Dictionary<string, string> SituacaoCadastral = new()
        {
            ["NULA"] = "01",
            ["ATIVA"] = "02",
            ["SUSPENSA"] = "03",
            ["INAPTA"] = "04",
            ["BAIXADA"] = "08"
        };
        private DateTime StringToDateTime(string date)
        {
            if (date.Count(c => c == '/') != 2)
                throw new Exception();
            var dateSplited = date.Split('/');
            if (dateSplited[0].Count() == 2 && dateSplited[1].Count() == 2 && dateSplited[2].Count() == 4)
            {
                return new DateTime(Int32.Parse(dateSplited[2]), Int32.Parse(dateSplited[1]), Int32.Parse(dateSplited[0]));
            }
            throw new Exception();
        }
        
        [HttpGet]
        public async Task<IActionResult> Search()
        {
            var collection = _context.Estabelecimento;
            var query = Request.Query;

            var filters = new List<FilterDefinition<Estabelecimento>>();
            var builder = Builders<Estabelecimento>.Filter;

            int page = query.ContainsKey("page") ? int.Parse(query["page"]) : 1;
            int pageSize = query.ContainsKey("pageSize") ? int.Parse(query["pageSize"]) : 20;

            foreach (var param in query)
            {
                List<FilterDefinition<Estabelecimento>> valuesList = new();
                var key = param.Key.ToLower();
                var values = param.Value.ToString().Split(';');

                switch (key)
                {
                    case "nomefantasia":
                    case "razaosocial":
                        foreach (var value in values)
                        {
                            valuesList.Add(builder.Eq("NomeFantasia", value));
                        }
                        break;

                    case "cnae":
                    case "cnaes":
                    case "atividade":
                    case "atividades":
                        foreach (var value in values)
                        {
                            valuesList.Add(builder.Eq("CnaePrincipal", value));
                            valuesList.Add(builder.Eq("CnaeSecundario", value)); // dar a opção de incluir ou não cnaes secundários
                        }
                        break;

                    case "naturezajuridica":
                        break;

                    case "situacaocadastral":
                        foreach (var value in values)
                        {
                            if (SituacaoCadastral.ContainsKey(value.ToUpper()))
                            {
                                valuesList.Add(builder.Eq("SituacaoCadastral", SituacaoCadastral[value.ToUpper()]));
                            }
                        }
                        break;

                    case "estado":
                    case "uf":
                        foreach (var value in values)
                        {
                            valuesList.Add(builder.Eq("UF", value));
                        }
                        break;

                    case "municipio":
                        foreach (var value in values)
                        {
                            valuesList.Add(builder.Eq("Municipio", value));
                        }
                        break;

                    case "bairro":
                        foreach (var value in values)
                        {
                            valuesList.Add(builder.Eq("Bairro", value));
                        }
                        break;

                    case "cep":
                        foreach (var value in values)
                        {
                            valuesList.Add(builder.Eq("CEP", value));
                        }
                        break;

                    case "ddd":
                        foreach (var value in values)
                        {
                            valuesList.Add(builder.Eq("Ddd1", value));
                            valuesList.Add(builder.Eq("Ddd2", value));
                            valuesList.Add(builder.Eq("Ddd3", value));
                        }
                        break;

                    case "dataabertura":
                    case "datadeabertura":
                    case "datainicioatividade":
                        foreach (var value in values)
                        {
                            try
                            {
                                switch (value.Count(c => c == ':'))
                                {
                                    case 1:
                                        var dateRange = value.Split(':');
                                        var dateStart = StringToDateTime(dateRange[0]);
                                        var dateEnd = StringToDateTime(dateRange[1]);
                                        valuesList.Add(builder.And(builder.Gte("DataInicioAtividade", dateStart), builder.Lte("DataInicioAtividade", dateEnd)));
                                        break;
                                    case 0:
                                        var date = StringToDateTime(value);
                                        valuesList.Add(builder.Gte("DataInicioAtividade", date));
                                        break;
                                }
                            }
                            catch (Exception) { }
                        }
                        break;

                    case "capitalsocial": //tá em outra tabela
                        foreach (var value in values)
                        {
                            try
                            {
                                var capitalString = value.Replace(',', '.');
                                switch (capitalString.Count(c => c == ':'))
                                {
                                    case 1:
                                        var capitalRange = capitalString.Split(':');
                                        List<FilterDefinition<Estabelecimento>> capitalList = new();
                                        if (capitalRange[0].Count() > 0)
                                            capitalList.Add(builder.Gte("CapitalSocial", double.Parse(capitalRange[0])));
                                        if (capitalRange[1].Count() > 0)
                                            capitalList.Add(builder.Lte("CapitalSocial", double.Parse(capitalRange[1])));
                                        if(capitalList.Count() > 0)
                                            valuesList.Add(builder.And(capitalList));
                                        break;
                                    case 0:
                                        var capital = double.Parse(capitalString);
                                        valuesList.Add(builder.Gte("CapitalSocial", capital));
                                        break;
                                }
                            }
                            catch (Exception) { }
                        }
                        break;

                    case "mei":
                        break;

                    case "matrizfilial":
                        switch (values[0].ToLower())
                        {
                            case "matriz":
                            case "1":
                            case "01":
                                filters.Add(builder.Eq("MatrizFilial", "1"));
                                break;
                            case "filial":
                            case "2":
                            case "02":
                                filters.Add(builder.Eq("MatrizFilial", "2"));
                                break;
                        }
                        break;

                    case "page":
                        break; 
                    case "pageSize":
                        break;
                    default:
                        break;
                }
                if(valuesList.Count() > 0)
                    filters.Add(builder.Or(valuesList));
            }

            var finalFilter = filters.Count > 0
                ? builder.And(filters)
                : builder.Empty;

            // Total antes da paginação
            long totalItems = await collection.CountDocumentsAsync(finalFilter);

            // Aplicando paginação
            var results = await collection
                .Find(finalFilter)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();


            var response = new ResponseDTO
            {
                Success = true,
                Message = "Busca realizada com sucesso.",
                Data = results,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            };

            return Ok(response);
        }

        [HttpGet("LeadsGraph")]
        public async Task<IActionResult> LeadsGraph()
        {
            // Gráfico: Quantidade de estabelecimentos por município

            // Busca o total por código do município
            var totalPorMunicipio = await _context.Estabelecimento.Aggregate()
                .Group(e => e.Municipio, g => new
                {
                    MunicipioId = g.Key,
                    Quantidade = g.Count()
                })
                .SortByDescending(x => x.Quantidade)
                .Limit(5)
                .ToListAsync();

            // Busca os nomes dos municípios
            var municipioIds = totalPorMunicipio.Select(t => t.MunicipioId).ToList();
            var municipios = await _context.Municipio
                .Find(m => municipioIds.Contains(m._id))
                .ToListAsync();

            // Combina ambos
            var resultado = totalPorMunicipio.Select(total => new
            {
                municipio = municipios.FirstOrDefault(m => m._id == total.MunicipioId)?.Descricao ?? total.MunicipioId,
                quantidade = total.Quantidade
            })
            .OrderByDescending(x => x.quantidade)
            .ToList();

            if (!resultado.Any())
            {
                return Ok(new { message = "Nenhum dado encontrado" });
            }

            return Ok(resultado);
        }
        
        
        [HttpGet("{cnpj}")]
        public async Task<IActionResult> GetByCnpj(string cnpj)
        {
            cnpj = new string(cnpj.Where(char.IsDigit).ToArray());

            if (cnpj.Length != 14)
                return BadRequest(new { success = false, message = "CNPJ inválido." });

            var cnpjBase = cnpj.Substring(0, 8);
            var cnpjOrdem = cnpj.Substring(8, 4);
            var cnpjDV = cnpj.Substring(12, 2);

            var lead = await _context.Estabelecimento.Find(e =>
                    e.CnpjBase == cnpjBase &&
                    e.CnpjOrdem == cnpjOrdem &&
                    e.CnpjDV == cnpjDV  
            ).FirstOrDefaultAsync();

            if (lead == null)
                return NotFound(new { success = false, message = "Lead não encontrado." });

            return Ok(new { success = true, data = lead });
        }

    }
}