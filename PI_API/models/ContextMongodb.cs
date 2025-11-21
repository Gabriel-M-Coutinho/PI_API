using LeadSearch.Models;
using MongoDB.Driver;
using PI_API.models.leads;

namespace PI_API.models
{
    public class ContextMongodb
    {
        public static string? ConnectionString { get; set; }
        public static string? Database { get; set; }
        public static bool IsSSL { get; set; }
        private IMongoDatabase _database { get; }


        public ContextMongodb()
        {
            try
            {
                MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(ConnectionString));
                if (IsSSL)
                {
                    settings.SslSettings = new SslSettings { EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12 };
                }
                var mongoCliente = new MongoClient(settings);
                _database = mongoCliente.GetDatabase(Database);
            }
            catch (Exception)
            {
                throw new Exception("Não foi possível conectar Mongodb");
            }

        }//fim do contrutor
        public IMongoCollection<ApplicationUser> User
        {
            get
            {
                return _database.GetCollection<ApplicationUser>("User");
            }
        }
        public IMongoCollection<Cnae> Cnae
        {
            get
            {
                return _database.GetCollection<Cnae>("Cnaes");
            }
        }
        public IMongoCollection<Empresa> Empresa
        {
            get
            {
                return _database.GetCollection<Empresa>("Empresas");
            }
        }
        public IMongoCollection<Estabelecimento> Estabelecimento
        {
            get
            {
                return _database.GetCollection<Estabelecimento>("Estabelecimentos");
            }
        }

        public IMongoCollection<Municipio> Municipio
        {
            get
            {
                return _database.GetCollection<Municipio>("Municipios");
            }
        }
    }//fim da classe
}