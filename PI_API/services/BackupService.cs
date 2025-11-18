using System.Diagnostics;
using System.Collections.Generic;

namespace PI_API.services;

public class BackupService
{
    private const string MongoDumpPath = "mongodump";
    private const string MongoConnectionString = "mongodb://localhost:27017";
    private const string DatabaseName = "ApiDB";
    private const string BackupDirectory = "C:\\Users\\0201392321040\\Desktop\\PI_API\\PI_API\\backup";
    private static readonly List<string> BackupCollections = new List<string> { "User", "Order" };

    private static string GetBaseBackupPathWithTimestamp()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        return Path.Combine(BackupDirectory, $"dump_{timestamp}");
    }

    public static async Task RunBackup()
    {
        string baseOutputDirectory = GetBaseBackupPathWithTimestamp();

        Directory.CreateDirectory(baseOutputDirectory);
        Console.WriteLine($"Iniciando backup para o diretório base: {baseOutputDirectory}");

        foreach (var collectionName in BackupCollections)
        {
            string outputDirectory = baseOutputDirectory;

            // --uri (conexão) --db (banco) --collection (nome da coleção) -o (diretório de saída)
            string arguments = $@"--uri ""{MongoConnectionString}"" --db ""{DatabaseName}"" --collection ""{collectionName}"" -o ""{outputDirectory}""";

            Console.WriteLine($"\n--- Executando backup da coleção: {collectionName} ---");
            Console.WriteLine($"Comando: {MongoDumpPath} {arguments}");

            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = MongoDumpPath,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var process = new Process { StartInfo = processStartInfo };

                process.Start();

                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    Console.WriteLine($"Backup da coleção '{collectionName}' concluído com sucesso.");
                    Console.WriteLine("Detalhes da Saída:");
                    Console.WriteLine(output);
                }
                else
                {
                    Console.WriteLine($"Erro no Backup da coleção '{collectionName}'. Código de saída: {process.ExitCode}");
                    Console.WriteLine("Detalhes do Erro:");
                    Console.WriteLine(error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocorreu uma exceção ao tentar executar o mongodump para {collectionName}: {ex.Message}");
                Console.WriteLine("Verifique se o 'mongodump' está no PATH.");
            }
        }
        Console.WriteLine($"\n=== Todos os backups das coleções em {baseOutputDirectory} foram concluídos. ===");
    }
}