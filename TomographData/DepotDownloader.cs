using System.Diagnostics;
using Tiger;

namespace TomographData;

public class DepotDownloader
{
    struct DDCredentials
    {
        public string Username { get;}
        public string Password { get;}
        
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
        await File.WriteAllLinesAsync(Path.Combine(depotDownloaderDirectory, fileListName), fileList);
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
        
        await RunProcessAsync("dotnet", depotDownloaderDirectory, arguments);
    }
    
    static Task<int> RunProcessAsync(string fileName, string workingDirectory, List<string> argumentList)
    {
        var tcs = new TaskCompletionSource<int>();

        var process = new Process
        {
            StartInfo = { FileName = fileName, Arguments = string.Join(" ", argumentList), WorkingDirectory = workingDirectory},
            EnableRaisingEvents = true
        };

        process.Exited += (sender, args) =>
        {
            tcs.SetResult(process.ExitCode);
            process.Dispose();
        };

        process.Start();
        
        return tcs.Task;
    }

    public void SetCredentials(string username, string password)
    {
        Credentials = new DDCredentials(username, password);
    }
}