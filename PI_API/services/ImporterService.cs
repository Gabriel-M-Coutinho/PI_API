using AspNetCore.Identity.MongoDbCore.Infrastructure;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using PI_API.models.leads;
using PI_API.settings;
using SharpCompress.Common;
using SharpCompress.Writers;
using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;

namespace PI_API.services
{
    public class ImporterService
    {

        private readonly Dictionary<string, int> ComputerConfig = new()
        {
            ["Cores"] = Environment.ProcessorCount,
            ["RAM"] = 0,
            ["Disk"] = 0
        };
        //Configurações para conexão com o MongoDB
        //OBS: posteriormente não ficará aqui por questões de segurança
        private MongoDbSettings _mongoDbSettings;
        public ImporterService(MongoDbSettings mongoDbSettings)
        {
             _mongoDbSettings = mongoDbSettings;
            mongoDatabase = new MongoClient(_mongoDbSettings.ConnectionString).GetDatabase(_mongoDbSettings.DatabaseName);
        }

        private readonly SemaphoreSlim downloadSemaphore = new(3);
        private readonly string[] filesArray = ["Cnaes",/* "Empresas0", "Empresas1", "Empresas2", "Empresas3", "Empresas4","Empresas5", "Empresas6", "Empresas7","Empresas8", */"Empresas9",/*"Estabelecimentos0","Estabelecimentos1","Estabelecimentos2","Estabelecimentos3","Estabelecimentos4","Estabelecimentos5","Estabelecimentos6","Estabelecimentos7","Estabelecimentos8",*/"Estabelecimentos9", "Motivos", "Municipios", "Naturezas", "Paises", "Qualificacoes", "Simples",/*"Socios0","Socios1","Socios2","Socios3","Socios4","Socios5","Socios6","Socios7", "Socios8", */"Socios9"];
        //Conexão com o MongoDB
        private readonly IMongoDatabase mongoDatabase;
        private readonly HttpClient httpClient = new(new HttpClientHandler
        {
            SslProtocols = System.Security.Authentication.SslProtocols.Tls12
        })
        {
            Timeout = TimeSpan.FromMinutes(10),
            DefaultRequestVersion = HttpVersion.Version20 // Melhor desempenho HTTP/2
        };
        private readonly Encoding Latin1Encoding = Encoding.GetEncoding("ISO-8859-1");

        private string baseUrl = "https://arquivos.receitafederal.gov.br/dados/cnpj/dados_abertos_cnpj/";
        private readonly Dictionary<string, string[]> headers = new()
        {
            ["Cnaes"] = ["_id", "Descricao"],
            ["Empresas"] = ["CnpjBase", "RazaoSocial", "NaturezaJuridica", "QualificacaoResponsavel", "CapitalSocial", "PorteEmpresa", "EnteFederativo"],
            ["Estabelecimentos"] = ["CnpjBase", "CnpjOrdem", "CnpjDV", "MatrizFilial", "NomeFantasia", "SituacaoCadastral", "DataSituacaoCadastral", "MotivoSituacaoCadastral", "CidadeExterior", "Pais", "DataInicioAtividade", "CnaePrincipal", "CnaeSecundario", "TipoLogradouro", "Logradouro", "Numero", "Complemento", "Bairro", "CEP", "UF", "Municipio", "Ddd1", "Telefone1", "Ddd2", "Telefone2", "DddFAX", "FAX", "CorreioEletronico", "SituacaoEspecial", "DataSituacaoEspecial"],
            ["Motivos"] = ["_id", "Descricao"],
            ["Municipios"] = ["_id", "Descricao"],
            ["Naturezas"] = ["_id", "Descricao"],
            ["Paises"] = ["_id", "Descricao"],
            ["Qualificacoes"] = ["_id", "Descricao"],
            ["Simples"] = ["CnpjBase", "OpcaoDoSimples", "DataOpcaoDoSimples", "DataExclusaoDoSimples", "MEI", "DataOpcaoMEI", "DataExclusaoMei"],
            ["Socios"] = ["CnpjBase", "IdentificadorSocio", "NomeSocio", "CnpjCpf", "QualificacaoSocio", "DataEntradaSociedade", "Pais", "RepresentanteLegal", "NomeRepresentante", "QualificacaoResponsavel", "FaixaEtaria"]
        };

        /* CÓDIGO (Métodos Públicos)
           * Start(): Começa do Zero ou Continua de onde parou
           * Pause(): Pausa todo o Processo
           * Restart(): Começa tudo de novo
           * Cancel(): Cancela a execução do Processo
           * Progress(): Devolve um objeto/dicionario com o progresso da execução até o momento (Arquivos baixados, Tempo Decorrido até o momento)
           * Log(): Retorna as Mensagens de Erro
           * Config(): Para configurar alguma coisa da importação (ConnectionDatabaseConfig, Limitação dos recursos disponíveis, etc...)
         */
        public async Task Start()
        /* [Start]
        - Será o Main dessa Classe
        - Terá como Papel chamar os metodos para executar o processo de todos os arquivos */
        {
            Console.WriteLine("# Iniciando ReceitaImporter #");
            /*if (!(await CheckMongoConnection()))
            {
                return;
            }*/
            await DropAllCollectionsAsync();
            var processFiles = new List<Task>();
            var sw = new Stopwatch();
            sw.Start();
            foreach (var fileName in filesArray)
            {
                await downloadSemaphore.WaitAsync();
                processFiles.Add(Task.Run(async () =>
                {
                    try
                    {
                        await ProcessFile(fileName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao processar {fileName}: {ex.Message}");
                    }
                    finally
                    {
                        downloadSemaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(processFiles);
            sw.Stop();
            Console.WriteLine("# ReceitaImporter Finalizado #");
            Console.WriteLine($"Tempo Decorrido: {sw.Elapsed.Hours}h {sw.Elapsed.Minutes}m {sw.Elapsed.Seconds}s {sw.Elapsed.Milliseconds}ms");
            await CreateIndexes();
        }

        public async Task CreateIndexes()
        {
            var collection = mongoDatabase.GetCollection<Estabelecimento>("Estabelecimentos");
            var indexKeys = Builders<Estabelecimento>.IndexKeys;
            List<IndexKeysDefinition<Estabelecimento>> indexList = [indexKeys.Ascending(e => e.Bairro), indexKeys.Ascending(e => e.CEP), indexKeys.Ascending(e => e.CnaePrincipal), indexKeys.Ascending(e => e.CnaeSecundario), indexKeys.Ascending(e => e.DataInicioAtividade), indexKeys.Ascending(e => e.Ddd1), indexKeys.Ascending(e => e.MatrizFilial), indexKeys.Ascending(e => e.Municipio), indexKeys.Ascending(e => e.NomeFantasia), indexKeys.Ascending(e => e.UF)]; 
            foreach(var index in indexList) 
            {
                await collection.Indexes.CreateOneAsync(new CreateIndexModel<Estabelecimento>(index));
            }
            Console.WriteLine("Indices Criados com sucesso!");
            
        }

        private async Task ProcessFile(string fileName)
        /* [ProcessDocument]
        - Método Responsavel pelo processo completo (download -> processamento -> importação) de um único
        - Também é onde ficarão os Canais (Channel)
        - Fará a Chamada dos Produtores e Consumidores 
        - Fará o Controle dinámico dos produtores e consumidores com base nos recursos disponiveis durante a execução (adição ou retiragem)
        - Adicionar/Retirar trabalhadores com base na necessidade, se quem estiver causando o gargalo for o banco adicionar mais no banco*/
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            List<string> ErrorLines = new List<string>();
            var file = baseUrl + DateTime.Now.ToString("yyyy-MM") + @"\" + fileName + ".zip";
            var channelOptions = new BoundedChannelOptions(50000) { FullMode = BoundedChannelFullMode.Wait }; //quando encher, começar a escrever no disco para não atrapalhar o download, ou alguma outra etapa
            fileName = Regex.Replace(fileName, @"[0-9]", "");
            var thisHeaderCollection = headers[fileName];

            var DataDownload = Channel.CreateBounded<byte[]>(channelOptions);
            var DataProcess = Channel.CreateBounded<BsonDocument>(channelOptions);

            ICollection<Task> downloaders = new List<Task>();
            ICollection<Task> processors = new List<Task>();
            ICollection<Task> importers = new List<Task>();
            List<byte[]> Chunks = new();
            const int batchSize = 5000;

            const int BufferSize = 8 * 1024; // 8KB por leitura

            var sw = new Stopwatch();
            sw.Start();
            HttpResponseMessage response;
            while (true)
            {
                try
                {
                    response = await httpClient.GetAsync(file, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            for (int i = 0; i < 1; i++)
            {
                downloaders.Add(Downloader(DataDownload.Writer, token));
            }
            for (int i = 0; i < 3; i++)
            {
                processors.Add(Processor(DataDownload.Reader, DataProcess.Writer, token));
            }

            var opts = new InsertManyOptions { IsOrdered = false };
            var collection = mongoDatabase.GetCollection<BsonDocument>(fileName);
            int collectionSize = 0;
            for (int i = 0; i < 1; i++)
            {
                importers.Add(Importer(DataProcess.Reader, token));
            }
            await Task.WhenAll(downloaders);
            DataDownload.Writer.Complete();
            await Task.WhenAll(processors);
            DataProcess.Writer.Complete();
            await Task.WhenAll(importers);

            sw.Stop();
            Console.WriteLine($"Arquivo {fileName} Importado com Sucesso em {sw.Elapsed.Hours}h {sw.Elapsed.Minutes}m {sw.Elapsed.Seconds}s {sw.Elapsed.Milliseconds}ms!");

            if (ErrorLines.Count() > 0)
            {
                var path = AppDomain.CurrentDomain.BaseDirectory + @"ReiceitaImporterLog\";
                Directory.CreateDirectory(path);
                path += $"ErrorLines{fileName}.txt";
                foreach (string content in ErrorLines)
                {
                    File.AppendAllText(path, $"{content}\n");
                }
                Console.WriteLine($"Arquivo ErrorLines{fileName}.txt criado/atualizado");
            }

            async Task Downloader(ChannelWriter<byte[]> writer, CancellationToken token)
            /* [Downloader]
            - Produtor do Canal DataDownload
            - Irá realizar o download dos arquivos armazenando na RAM em Stream
            - Irá extrair
            - Ele irá armazenar em blocos (bytes), e irá adicionar esse blocos no Canal DataDownload
            - Terá provavelmente apenas 1, para baixar o arquivo inteiro, ou mais para realizar o download em partes 
            (só terá mais se tiver mais recurso disponivel mesmo baixando 3 arquivos simultaneamente)*/
            {
                byte[] buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
                try
                {
                    await using var zipStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    using var zipInputStream = new ZipInputStream(zipStream);
                    zipInputStream.IsStreamOwner = false;

                    ZipEntry entry;
                    while ((entry = zipInputStream.GetNextEntry()) != null)
                    {
                        if (!entry.IsFile) continue;
                        Console.WriteLine($"Lendo: {entry.Name}");

                        int bytesRead;
                        int totalChunk;
                        int remaning = 0;

                        // utilizar o PipeReader invés do ReadAsync (pesquisar mais sobre)
                        while ((bytesRead = await zipInputStream.ReadAsync(buffer, remaning, BufferSize - remaning)) > 0)
                        {
                            if (token.IsCancellationRequested) return;

                            buffer.AsSpan(remaning + bytesRead).Clear(); //tem que ver se está certo
                            totalChunk = buffer.AsSpan().LastIndexOf((byte)'\n') + 1; //quantidade, mas tbm significa o inicio da ultima linha
                                                                                      // Copia apenas os dados lidos em um novo buffer
                            byte[] chunk = ArrayPool<byte>.Shared.Rent(totalChunk); //pega um array com a quantidade do totalChunk
                            chunk.AsSpan(totalChunk).Clear(); //limpa o restante que veio do array pool

                            buffer.AsSpan(0, totalChunk).CopyTo(chunk); //transforma o buffer em uma view do 0 até o ultimo \n, já que aqui é o count, e não indice, e manda para chunk
                                                                        // Envia para o canal

                            await writer.WriteAsync(chunk);
                            remaning = buffer.Length - totalChunk;
                            buffer.AsSpan(totalChunk, remaning).CopyTo(buffer);
                            buffer.AsSpan(remaning + 1).Clear();
                        }
                    }
                }
                catch (OperationCanceledException) { /* cooperative cancellation */ }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }

            async Task Processor(ChannelReader<byte[]> reader,
                     ChannelWriter<BsonDocument> writer, CancellationToken token)
            {
                int fullLine;
                try {
                await foreach (var chunk in reader.ReadAllAsync())
                {
                    if (token.IsCancellationRequested) break;
                    ReadOnlyMemory<byte> chunkMemory = chunk;
                    while ((fullLine = chunkMemory.Span.IndexOf((byte)'\n')) >= 0)
                    {
                        fullLine++;
                        ReadOnlyMemory<byte> line = chunkMemory.Slice(0, fullLine);
                        chunkMemory = chunkMemory.Slice(fullLine);

                        if (chunkMemory.IsEmpty) continue;

                        try
                        {
                            BsonDocument doc = ParseCsvLine(line.Span);
                            await writer.WriteAsync(doc);
                        }
                        catch (Exception ex)
                        {
                            ErrorLines.Add(ex.Message);
                        }
                    }
                    ArrayPool<byte>.Shared.Return(chunk);
                }
                }
                catch (OperationCanceledException) { }
            }
            BsonDocument ParseCsvLine(ReadOnlySpan<byte> line)
            {
                var doc = new BsonDocument();

                int start = 0;
                int headerIdx = 0;

                try
                {
                    for (int i = 0; i < line.Length; i++)
                    {
                        if (line[i] == (byte)';' && line[i - 1] == (byte)'\"' && line[i + 1] == (byte)'\"')
                        {
                            addFieldToDocument(line, start, i, headerIdx);
                            start = i + 1;
                            headerIdx++;
                        }
                    }
                    addFieldToDocument(line, start, line.Length - 1, headerIdx);
                }
                catch (Exception)
                {
                    throw new Exception(Latin1Encoding.GetString(line));
                }
                return doc;
                void addFieldToDocument(ReadOnlySpan<byte> line, int start, int end, int headerIdx)
                {
                    ReadOnlySpan<byte> fieldSpan = line.Slice(start + 1, (end - start) - 2);
                    try
                    {
                        switch (fileName)
                        {
                            case "Empresas":
                                DatabaseFormater.Empresas(doc, fieldSpan, headerIdx);
                                break;
                            case "Estabelecimentos":
                                DatabaseFormater.Estabelecimentos(doc, fieldSpan, headerIdx);
                                break;
                            case "Socios":
                                DatabaseFormater.Socios(doc, fieldSpan, headerIdx);
                                break;
                            case "Simples":
                                DatabaseFormater.Simples(doc, fieldSpan, headerIdx);
                                break;
                            default:
                                doc[thisHeaderCollection[headerIdx]] = Latin1Encoding.GetString(fieldSpan);
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"Deu Erro:\n- Linha: {Latin1Encoding.GetString(line)}\n- Campo: {Latin1Encoding.GetString(fieldSpan)}");
                        doc[thisHeaderCollection[headerIdx]] = Latin1Encoding.GetString(fieldSpan);
                    }
                }
            }

            
            async Task Importer(ChannelReader<BsonDocument> reader, CancellationToken token)
            /* [Importer]
            - Consumidor do Canal DataProcess
            - Irá realizar a importação dos Bson Document para o MongoDB
            - Provavel que terá mais de um para esse processo*/
            {
                List<BsonDocument> batch = new();
                try {
                await foreach (var doc in reader.ReadAllAsync())
                {
                    //Console.WriteLine(fileName);
                    if (collectionSize >= 1000000)
                    {
                        // stop everything:
                        cts.Cancel();                 // signal cancellation
                        DataProcess.Writer.Complete(); // close pipes (only once - see below)
                        DataDownload.Writer.Complete();
                        return;
                    }

                    batch.Add(doc);
                    if (batch.Count >= batchSize)
                    {
                        //Console.WriteLine("Vou inserir");
                        await collection.InsertManyAsync(batch, opts);
                        batch.Clear();
                        collectionSize += batchSize;
                    }
                }
                if (batch.Count > 0)
                    await collection.InsertManyAsync(batch);
                }
                catch (OperationCanceledException) { }
            }
        }

        private async Task DropAllCollectionsAsync()
        {
            Console.WriteLine("|=====| INICIANDO LIMPEZA DO BANCO DE DADOS |=====|");
            string[] collectionNames = ["Cnaes", "Empresas", "Estabelecimentos", "Motivos", "Municipios", "Naturezas", "Paises", "Qualificacoes", "Simples", "Socios"];
            var colectionsDatabase = mongoDatabase.ListCollectionNames().ToList();

            if (!colectionsDatabase.Any(e => collectionNames.Contains(e)))
            {
                Console.WriteLine("Nenhuma coleção encontrada para apagar.");
            }
            else
            {
                foreach (var collectionName in collectionNames)
                {
                    try
                    {
                        Console.WriteLine($" - Apagando coleção: {collectionName}");
                        await mongoDatabase.DropCollectionAsync(collectionName);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Não deu para apagar a Collection " + collectionName);
                    }
                }
            }
            Console.WriteLine("|=====| LIMPEZA DO BANCO DE DADOS CONCLUÍDA |=====|\n");
        }

        // CheckConnection

        public async Task<bool> CheckMongoConnection()
        {
            try
            {
                var settings = MongoClientSettings.FromConnectionString(_mongoDbSettings.ConnectionString);
                settings.ConnectTimeout = TimeSpan.FromSeconds(5);
                settings.SocketTimeout = TimeSpan.FromSeconds(5);
                settings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);

                var databaseTeste = new MongoClient(settings).GetDatabase(_mongoDbSettings.DatabaseName);

                await databaseTeste.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
                Console.WriteLine("- Sucessfully MongoDB Conection");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"- Erro MongoDB: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CheckHttpConnection()
        {
            try
            {
                var response = await httpClient.GetAsync(baseUrl);
                Console.WriteLine("- Sucessfully Http Conection");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"- Erro Http: {ex.Message}");
                return false;
            }
        }

        // Melhorar Retry

        private async Task<T> RetryAsync<T>(Func<Task<T>> action, int maxAttempts, int delayMs = 2000)
        {
            List<Exception> exceptions = new();
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Falha na {attempt}° tentativa");
                    exceptions.Add(ex);
                    if (attempt < maxAttempts - 1)
                    {
                        await Task.Delay(delayMs * (int)Math.Pow(2, attempt)); // Backoff exponencial
                    }
                }
            }
            throw new AggregateException($"Falha após {maxAttempts} tentativas.", exceptions);
        }
    }
}
