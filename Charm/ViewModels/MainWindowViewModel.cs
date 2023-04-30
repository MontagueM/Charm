using System;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Windows.Input;
using ReactiveUI;
using Tiger;

namespace Charm.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ICommand ExportMapToAtlas { get; }
    public ReactiveCommand<Unit, Unit> OpenHash { get; }
    public string Hash { get; set; } = "A405A080";

    public MainWindowViewModel()
    {
        ExportMapToAtlas = ReactiveCommand.CreateFromTask(async () =>
        {

        });

        OpenHash = ReactiveCommand.CreateFromTask(async () =>
        {
            Strategy.SetStrategy(TigerStrategy.DESTINY2_SHADOWKEEP_2601);
            var x = PackageResourcer.Get();
            byte[] data = PackageResourcer.Get().GetFileData(new FileHash(Hash));
            string tempFilePath = $"./TempFiles/{Hash}.bin";
            Directory.CreateDirectory(Path.GetDirectoryName(tempFilePath));
            File.WriteAllBytes(tempFilePath, data);
            new Process
            {
                StartInfo = new ProcessStartInfo($@"{Path.GetFullPath(tempFilePath)}")
                {
                    UseShellExecute = true
                }
            }.Start();
        });

        OpenHash.ThrownExceptions.Subscribe(new Action<object>(ex =>
        {
            Console.WriteLine(ex);
            Debug.WriteLine(ex);
            var a = 0;
        }));
    }

}
