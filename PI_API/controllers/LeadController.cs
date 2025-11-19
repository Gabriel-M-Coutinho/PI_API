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

            foreach (var param in query)
            {
                var key = param.Key.ToLower();
                var value = param.Value.ToString();

                Console.WriteLine(value);
                switch (key)
                {
                    case "nomefantasia":
                        filters.Add(builder.Eq("NomeFantasia", value));
                        break;

                    case "cnae":
                        filters.Add(builder.Eq("CnaePrincipal", value)); ;
                        break;

                    case "situacaocadastral":
                        filters.Add(builder.Eq("SituacaoCadastral", SituacaoCadastral[value]));
                        break;
                    default:
                        // ignorar params desconhecidos
                        break;
                }
            }

            var finalFilter = filters.Count() != 0
                ? builder.And(filters)
                : builder.Empty;

            var results = await collection.Find(_ => true).Limit(5).ToListAsync();
            Console.WriteLine(results.Count());
            foreach(var result in results)
            {
                Console.WriteLine(result.ToString());
            }
            return Ok(results.ToJson());
        }
    }
}