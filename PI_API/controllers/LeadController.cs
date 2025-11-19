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
                var key = param.Key.ToLower();
                var value = param.Value.ToString();

                switch (key)
                {
                    case "nomefantasia":
                        filters.Add(builder.Eq("NomeFantasia", value));
                        break;

                    case "cnae":
                        filters.Add(builder.Eq("CnaePrincipal", value));
                        break;

                    case "situacaocadastral":
                        if (SituacaoCadastral.ContainsKey(value.ToUpper()))
                        {
                            filters.Add(builder.Eq("SituacaoCadastral", SituacaoCadastral[value.ToUpper()]));
                        }
                        break;

                    case "page":
                        break; 
                    case "pageSize":
                        break; 
                }
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