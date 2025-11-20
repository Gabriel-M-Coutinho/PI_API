using LeadSearch.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDbGenericRepository;
using PI_API.dto;
using PI_API.models;
using PI_API.models.leads;
using PI_API.services;

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
                                switch (value.Count(c => c == '-'))
                                {
                                    case 1:
                                        var dateRange = value.Split('-');
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

                    case "capitalsocial":
                        break;

                    case "mei":
                        break;
                    case "matriz":
                        break;
                    case "filial":
                        break;
                    case "page":
                        break; 
                    case "pageSize":
                        break;
                    default:
                        break;
                }
                foreach (var valuesA in valuesList)
                {
                    Console.WriteLine(valuesList);
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
    }
}