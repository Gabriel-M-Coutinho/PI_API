using LeadSearch.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDbGenericRepository;
using PI_API.dto;
using PI_API.models;
using PI_API.models.leads;
using PI_API.services;
using System.Security.Claims;
using System.Text.Json;

namespace PI_API.controllers
{


    [Route("api/[controller]")]
    [ApiController]
    public class LeadController : ControllerBase
    {
        private readonly ContextMongodb _context;
        private readonly CreditService _creditService;
        private readonly UserManager<ApplicationUser> _userManager;

        // CONFIGURAÇÃO FLEXÍVEL - Pode vir de appsettings.json ou banco de dados
        private const int CREDITOS_POR_LEAD = 1; // Mude aqui para aumentar o valor
        public LeadController(ContextMongodb context, CreditService creditService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _creditService = creditService;
            _userManager = userManager;
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
[Authorize]
public async Task<IActionResult> Search()
{
    var userId = User.FindFirst("userid")?.Value;

    if (string.IsNullOrEmpty(userId)) return Unauthorized("Usuário não identificado.");

    var currentUser = await _userManager.FindByIdAsync(userId);

    if (currentUser == null) return Unauthorized("Usuário não encontrado.");

    var creditosAtuais = await _creditService.GetUserCredits(userId);

    if (creditosAtuais <= 0)
    {
        return BadRequest(new ResponseDTO
        {
            Success = false,
            Message = "Você não tem créditos suficientes para visualizar leads."
        });
    }

    var collection = _context.Estabelecimento;
    var query = Request.Query;

    var filters = new List<FilterDefinition<Estabelecimento>>();
    var builder = Builders<Estabelecimento>.Filter;

    // EXCLUSÃO DE CNPJs JÁ COMPRADOS - AGORA SIMPLIFICADO
    if (currentUser.CnpjsComprados != null && currentUser.CnpjsComprados.Any())
    {
        filters.Add(builder.Nin(e => e.CnpjCompleto, currentUser.CnpjsComprados));
    }

    int quantity = query.ContainsKey("quantity") ? int.Parse(query["quantity"]) : 0;

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
                    valuesList.Add(builder.Eq("CnaeSecundario", value));
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

            case "capitalsocial":
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
                                if (capitalList.Count() > 0)
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
        if (valuesList.Count() > 0)
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
        .Limit(quantity)
        .ToListAsync();

    // 2º - REDUZIR CRÉDITOS APÓS A BUSCA
    var creditosUsados = 0;
    if (!string.IsNullOrEmpty(userId) && results.Any())
    {
        Console.WriteLine("Entrei no primeiro IF de reduzir créditos");
        creditosUsados = results.Count * CREDITOS_POR_LEAD;
        var sucesso = await _creditService.UseCredits(userId, creditosUsados);

        if (!sucesso)
        {
            Console.WriteLine("Entrei no segundo IF de reduzir créditos");
            return BadRequest(new ResponseDTO
            {
                Success = false,
                Message = $"Créditos insuficientes. Esta busca custaria {creditosUsados} créditos."
            });
        }
    }

    // 3º - REGISTRAR LEADS COMPRADOS
    var userToUpdate = await _userManager.FindByIdAsync(userId);
    if (userToUpdate != null)
    {
        Console.WriteLine("Entrei no primeiro de registrar os Leads");
        foreach (var lead in results)
        {
            // AGORA É SÓ USAR O CAMPO DIRETO
            var cnpjCompleto = lead.CnpjCompleto;
            if (!userToUpdate.CnpjsComprados.Contains(cnpjCompleto))
            {
                Console.WriteLine("Entrei no segundo IF de registrar os Leads");
                userToUpdate.CnpjsComprados.Add(cnpjCompleto);
            }
        }
        await _userManager.UpdateAsync(userToUpdate);
    }

    var creditosRestantes = await _creditService.GetUserCredits(userId);

    var response = new ResponseDTO
    {
        Success = true,
        Message = "Busca realizada com sucesso.",
        Data = results,
        TotalItems = totalItems
    };

    return Ok(response);
}


[HttpGet]
[Authorize]
[Route("search-purchased")]
public async Task<IActionResult> SearchPurchased()
{
    var userId = User.FindFirst("userid")?.Value;

    if (string.IsNullOrEmpty(userId)) return Unauthorized("Usuário não identificado.");

    var currentUser = await _userManager.FindByIdAsync(userId);

    if (currentUser == null) return Unauthorized("Usuário não encontrado.");

    // VERIFICA SE O USUÁRIO TEM CNPJs COMPRADOS
    if (currentUser.CnpjsComprados == null || !currentUser.CnpjsComprados.Any())
    {
        return Ok(new ResponseDTO
        {
            Success = true,
            Message = "Nenhum CNPJ comprado encontrado para este usuário.",
            Data = new List<Estabelecimento>(),
            TotalItems = 0
        });
    }

    var collection = _context.Estabelecimento;
    var query = Request.Query;

    var filters = new List<FilterDefinition<Estabelecimento>>();
    var builder = Builders<Estabelecimento>.Filter;

    // FILTRO PRINCIPAL: APENAS CNPJs QUE O USUÁRIO JÁ COMPROU
    filters.Add(builder.In(e => e.CnpjCompleto, currentUser.CnpjsComprados));

    int page = query.ContainsKey("page") ? int.Parse(query["page"]) : 1;
    int pageSize = query.ContainsKey("pageSize") ? int.Parse(query["pageSize"]) : 20;
    int quantity = query.ContainsKey("quantity") ? int.Parse(query["quantity"]) : 0;

    // Se quantity for especificado, usa ele como pageSize (para compatibilidade)
    if (quantity > 0)
    {
        pageSize = quantity;
    }

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
                    valuesList.Add(builder.Regex("NomeFantasia", new MongoDB.Bson.BsonRegularExpression(value, "i")));
                }
                break;

            case "cnae":
            case "cnaes":
            case "atividade":
            case "atividades":
                foreach (var value in values)
                {
                    valuesList.Add(builder.Eq("CnaePrincipal", value));
                    valuesList.Add(builder.Eq("CnaeSecundario", value));
                }
                break;

            case "naturezajuridica":
                foreach (var value in values)
                {
                    valuesList.Add(builder.Eq("NaturezaJuridica", value));
                }
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

            case "capitalsocial":
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
                                if (capitalList.Count() > 0)
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
                foreach (var value in values)
                {
                    if (bool.TryParse(value, out bool isMei))
                    {
                        valuesList.Add(builder.Eq("MEI", isMei));
                    }
                }
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
            case "pageSize":
            case "quantity":
                // Já tratado acima
                break;
            default:
                break;
        }
        if (valuesList.Count() > 0)
            filters.Add(builder.Or(valuesList));
    }

    var finalFilter = filters.Count > 0
        ? builder.And(filters)
        : builder.Empty;

    // Total de itens
    long totalItems = await collection.CountDocumentsAsync(finalFilter);

    // Calcula o total de páginas
    int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

    // Busca os resultados com paginação
    var results = await collection
        .Find(finalFilter)
        .Skip((page - 1) * pageSize)
        .Limit(pageSize)
        .ToListAsync();

    var response = new ResponseDTO
    {
        Success = true,
        Message = $"Busca realizada com sucesso. Encontrados {results.Count} CNPJs dos seus leads comprados.",
        Data = results,
        Page = page,
        PageSize = pageSize,
        TotalItems = totalItems,
        TotalPages = totalPages
    };

    return Ok(response);
}

        [Authorize(Roles = "ADMIN")]
        [HttpGet("EstablishmentsGraph")]
        public async Task<IActionResult> EstablishmentsGraph()
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

        [Authorize(Roles = "ADMIN")]
        [HttpGet("OrdersGraph")]
        public async Task<IActionResult> OrdersGraph()
        {
            // Define o intervalo: últimas 24 horas
            var since = DateTime.UtcNow.AddHours(-24);

            // Agrupa por hora e conta a quantidade de pedidos pagos
            var ordersByHour = await _context.Order.Aggregate()
                .Match(o => o.CreatedAt >= since && o.IsPaid)
                .Group(o => new { Hour = o.CreatedAt.AddHours(-3).Hour }, g => new
                {
                    Hour = g.Key.Hour,
                    Count = g.Count()
                })
                .SortBy(x => x.Hour)
                .ToListAsync();

            // Garante que todas as horas existam no resultado (0-23)
            var hours = Enumerable.Range(0, 24).ToList();
            var result = hours.Select(h => new
            {
                hora = $"{h}:00",
                quantidade = ordersByHour.FirstOrDefault(o => o.Hour == h)?.Count ?? 0
            }).ToList();

            return Ok(result);
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

            LeadSearchDTO lead = await _context.Estabelecimento.Aggregate()
            .Match(e =>
                e.CnpjBase == cnpjBase &&
                e.CnpjOrdem == cnpjOrdem &&
                e.CnpjDV == cnpjDV
            )
            /*.Lookup( //é para vir um documento Empresa
                foreignCollectionName: "Empresas",
                localField: "CnpjBase",
                foreignField: "CnpjBase",
                @as: "Empresa"
            )
            .Unwind("Empresa")*/
            .Lookup( //é para vir um documento Cnae (_id e descricao)
                foreignCollectionName: "Cnaes",
                localField: "CnaePrincipal",
                foreignField: "_id",
                @as: "realCnaePrincipal"
            )
            .Unwind("realCnaePrincipal")
            .Lookup(//é para vir um array de documentos Cnaes (_id e descricao)
                foreignCollectionName: "Cnaes",
                localField: "CnaeSecundario",
                foreignField: "_id",
                @as: "realCnaeSecundario"
            )
            .Lookup(//é para vir um array de documentos tipo socio
                foreignCollectionName: "Socios",
                localField: "CnpjBase",
                foreignField: "CnpjBase",
                @as: "Socios"
            )
            /*.Lookup(//é para vir um documento do tipo Simples
                foreignCollectionName: "Simples",
                localField: "CnpjBase",
                foreignField: "CnpjBase",
                @as: "Simples"
            )
            .Unwind("Simples")
            .Limit(1)*/
            .As<LeadSearchDTO>()
            .FirstOrDefaultAsync();


            if (lead == null)
            {
                Console.WriteLine("Deu ruim aqui");
                return NotFound(new { success = false, message = "Lead não encontrado." });
            }

            return Ok(new { success = true, data = lead });
        }
        [HttpGet("infoFields")]
        public async Task<IActionResult> GetInfoFields()
        {
            var Cnaes = await _context.Cnae.Find(_ => true).SortBy(e => e.Descricao).ToListAsync();
            var Municipios = await _context.Municipio.Find(_ => true).SortBy(e => e.Descricao).ToListAsync();
            if(Cnaes == null || Municipios == null)
            {
                return BadRequest("Falha ao Carregar dados");
            }
            var Response = new InfoFieldsDTO(Cnaes, Municipios);
            return Ok(Response);
        }
    }
}