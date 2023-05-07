using System.Diagnostics;
using Tiger;

namespace TomographData;

public class DepotDownloader
{
    struct DDCredentials
    {
        public string Username { get; }
        public string Password { get; }

        public DDCredentials(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }

    private DDCredentials Credentials { get; set; }

    public async Task Download(DepotManifestVersion depotManifestVersion, string meaningfulOutputName, List<string> fileList)
    {
        string outputDirectory = Path.Join("../../../../", $"Tomograph/TestData/{meaningfulOutputName}/");
        outputDirectory = Path.GetFullPath(outputDirectory);
        Directory.CreateDirectory(outputDirectory);
        string depotDownloaderDirectory = Path.Join("../../../", "DepotDownloader");
        string fileListName = $"{meaningfulOutputName}_filelist.txt";
        File.WriteAllLines(Path.Combine(depotDownloaderDirectory, fileListName), fileList);
        List<string> arguments = new List<string>
        {
            " DepotDownloader.dll",
            "-app", depotManifestVersion.AppId.ToString(),
            "-depot", depotManifestVersion.DepotId.ToString(),
            "-manifest", depotManifestVersion.ManifestId.ToString(),
            "-dir", outputDirectory,
            "-filelist", fileListName,
            "-validate",
            "-username", Credentials.Username,
            "-password", Credentials.Password
        };

        if (!Path.Exists(Path.Join(depotDownloaderDirectory, "DepotDownloader.dll")))
        {
            throw new FileNotFoundException($"DepotDownloader.dll does not exist in {depotDownloaderDirectory}");
        }

        RunProcessAsync("dotnet", depotDownloaderDirectory, arguments);
        Console.WriteLine($"Finished downloading {meaningfulOutputName} test data.");
    }

    static void RunProcessAsync(string fileName, string workingDirectory, List<string> argumentList)
    {
        var process = new Process
        {
            StartInfo =
            {
                FileName = fileName,
                Arguments = string.Join(" ", argumentList),
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true

            },
            EnableRaisingEvents = true
        };

        process.OutputDataReceived += (s, e) =>
        {
            if (e.Data != null)
            {
                Console.WriteLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.WaitForExit();
    }

    public void SetCredentials(string username, string password)
    {
        Credentials = new DDCredentials(username, password);
    }
}
