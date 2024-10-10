using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Tiger;
using Tiger.Schema.Audio;

namespace Charm;

public partial class DialogueView : UserControl
{
    private Dialogue _dialogue;
    private DialogueD1 _dialogueD1;

    // Kind of a hacky way but it works
    private TagView _viewer;
    private VoicelineItem _activeItem;

    public DialogueView()
    {
        InitializeComponent();
    }

    public void Load(FileHash hash, TagView viewer)
    {
        List<dynamic?> result = new();
        _viewer = viewer;
        if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
        {
            _dialogueD1 = new DialogueD1(hash);
            result = _dialogueD1.Load();
        }
        else
        {
            _dialogue = new Dialogue(hash);
            result = _dialogue.Load();
        }

        GenerateUI(result);
    }

    private void GenerateUI(List<dynamic?> dialogueTree)
    {
        ListView.ItemsSource = GenerateUIRecursive(0, dialogueTree);
    }

    private ObservableCollection<VoicelineItem> GenerateUIRecursive(int recursionDepth, List<dynamic?> dialogueTree)
    {
        ObservableCollection<VoicelineItem> result = new ObservableCollection<VoicelineItem>();
        foreach (var dyn in dialogueTree)
        {
            if (dyn is List<dynamic?>)
            {
                ObservableCollection<VoicelineItem> res = GenerateUIRecursive(recursionDepth + 1, dyn);
                foreach (var q in res)
                {
                    result.Add(q);
                }
            }
            else
            {
                if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
                {
                    SAA078080 a = dyn;

                    if (a.Dialogue is null)
                        continue;

                    if (a.Strings is not null)
                        GlobalStrings.Get().AddStrings(a.Strings);

                    if (a.StringsF is not null)
                        GlobalStrings.Get().AddStrings(a.StringsF);

                    result.Add(new VoicelineItem
                    {
                        Narrator = GlobalStrings.Get().GetString(a.Narrator),
                        Voiceline = GlobalStrings.Get().GetString(a.VoiceLine),
                        Wem = a.Dialogue.TagData.Wems[0],
                        RecursionDepth = recursionDepth,
                        Duration = a.Dialogue.TagData.Wems[0].Duration
                    });

                    if (a.DialogueF is null)
                        continue;

                    // A lot of times the Male and Female voice lines are the exact same, so just skip
                    if (GlobalStrings.Get().GetString(a.VoiceLineF) == GlobalStrings.Get().GetString(a.VoiceLine))
                        continue;

                    result.Add(new VoicelineItem
                    {
                        Narrator = GlobalStrings.Get().GetString(a.Narrator),
                        Voiceline = GlobalStrings.Get().GetString(a.VoiceLineF),
                        Wem = a.DialogueF.TagData.Wems[0],
                        RecursionDepth = recursionDepth,
                        Duration = a.DialogueF.TagData.Wems[0].Duration
                    });
                }
                else
                {
                    D2Class_33978080 entry = dyn;
                    result.Add(new VoicelineItem
                    {
                        Narrator = GlobalStrings.Get().GetString(entry.NarratorString),
                        Voiceline = entry.GetVoiceline(),
                        Wem = entry.SoundM.TagData.Wems[0],
                        RecursionDepth = recursionDepth,
                        Duration = entry.SoundM.TagData.Wems[0].Duration
                    });
                }
            }
        }

        // Filter out duplicates
        return new ObservableCollection<VoicelineItem>(result.GroupBy(x => x.Wem.Hash)
                                                      .Select(group => group.First()));
    }

    private void PlayWem_OnClick(object sender, RoutedEventArgs e)
    {
        VoicelineItem item = (VoicelineItem)(sender as Button).DataContext;
        _activeItem = item;
        MusicPlayer.SetWem(item.Wem);
        MusicPlayer.Play();

        if (_viewer is not null)
        {
            _viewer.ExportControl.SetExportFunction(ExportWav, (int)ExportTypeFlag.Full);
            _viewer.ExportControl.SetExportInfo(item.Narrator, MusicPlayer.GetWem());
        }
    }

    private void ExportWav(ExportInfo info)
    {
        // exporting while playing the audio causes a hang
        Dispatcher.Invoke(() =>
        {
            if (MusicPlayer.IsPlaying())
                MusicPlayer.Pause();
        });

        ConfigSubsystem config = CharmInstance.GetSubsystem<ConfigSubsystem>();
        Wem wem = FileResourcer.Get().GetFile<Wem>(info.Hash);
        string saveDirectory = config.GetExportSavePath() + $"/Sound/Dialogue/{info.Name}/";
        Directory.CreateDirectory(saveDirectory);
        wem.SaveToFile($"{saveDirectory}/{info.Hash}.wav");

        StringBuilder dialogueBuilder = new StringBuilder();
        if (File.Exists($"{saveDirectory}/Dialogue.txt"))
        {
            using (StreamReader reader = new StreamReader($"{saveDirectory}/Dialogue.txt"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line != $"[{info.Hash}]: \"{_activeItem.Voiceline}\"")
                        dialogueBuilder.AppendLine(line);
                }
            }
        }
        dialogueBuilder.AppendLine($"[{info.Hash}]: \"{_activeItem.Voiceline}\"");
        File.WriteAllText($"{saveDirectory}/Dialogue.txt", dialogueBuilder.ToString());
    }
}

public class VoicelineItem
{
    public string Narrator { get; set; }

    public string Voiceline { get; set; }

    public Wem Wem { get; set; }

    public int RecursionDepth { get; set; }

    public string Duration { get; set; }

    public Thickness Padding  // todo make this work nicely
    {
        get => new Thickness(Convert.ToDouble(RecursionDepth * 50 - 50), 0, 0, 0);
    }
}
