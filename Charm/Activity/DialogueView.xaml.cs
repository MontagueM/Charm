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
        if (Strategy.IsD1())
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

    public static ObservableCollection<VoicelineItem> Load(FileHash hash)
    {
        List<dynamic?> result = new();
        if (Strategy.IsD1())
        {
            DialogueD1 dialogueD1 = new DialogueD1(hash);
            result = dialogueD1.Load();
        }
        else
        {
            Dialogue dialogue = FileResourcer.Get().GetFile<Dialogue>(hash, shouldCache: false);
            result = dialogue.Load();
        }
        return GenerateUIRecursive(0, result);
    }

    public static ObservableCollection<VoicelineItem> GenerateUIRecursive(int recursionDepth, List<dynamic?> dialogueTree)
    {
        ObservableCollection<VoicelineItem> result = new();
        foreach (dynamic? dyn in dialogueTree)
        {
            if (dyn is List<dynamic?>)
            {
                ObservableCollection<VoicelineItem> res = GenerateUIRecursive(recursionDepth + 1, dyn);
                foreach (VoicelineItem q in res)
                {
                    result.Add(q);
                }
            }
            else
            {
                if (Strategy.IsD1())
                {
                    S808007AA a = dyn;

                    if (a.Dialogue is null || !a.Dialogue.TagData.Wems.Any())
                        continue;

                    if (a.Strings is not null)
                        GlobalStrings.Get().AddStrings(a.Strings);

                    if (a.StringsF is not null)
                        GlobalStrings.Get().AddStrings(a.StringsF);

                    var narrator = GlobalStrings.Get().GetString(a.Narrator);
                    var voiceline = GlobalStrings.Get().GetString(a.VoiceLine);

                    foreach (var wem in a.Dialogue.TagData.Wems)
                    {
                        if (wem.GetReferenceHash().IsInvalid())
                            continue;

                        result.Add(new VoicelineItem
                        {
                            Narrator = narrator,
                            Voiceline = voiceline,
                            WemHash = wem.Hash,
                            RecursionDepth = recursionDepth,
                        });
                    }

                    if (a.DialogueF is null)
                        continue;

                    // A lot of times the Male and Female voice lines are the exact same, so just skip
                    if (GlobalStrings.Get().GetString(a.VoiceLineF) == GlobalStrings.Get().GetString(a.VoiceLine))
                        continue;

                    var voicelineF = GlobalStrings.Get().GetString(a.VoiceLineF);
                    foreach (var wem in a.DialogueF.TagData.Wems)
                    {
                        if (wem.GetReferenceHash().IsInvalid())
                            continue;

                        result.Add(new VoicelineItem
                        {
                            Narrator = narrator,
                            Voiceline = voicelineF,
                            WemHash = wem.Hash,
                            RecursionDepth = recursionDepth,
                        });
                    }
                }
                else
                {
                    S80809733 entry = dyn;
                    if (entry.SoundM is null || !entry.SoundM.TagData.Wems.Any())
                        continue;

                    foreach (var wem in entry.SoundM.TagData.Wems)
                    {
                        if (wem.GetReferenceHash().IsInvalid())
                            continue;

                        result.Add(new VoicelineItem
                        {
                            Narrator = entry.GetNarratorString(),
                            Voiceline = entry.GetVoiceline(),
                            WemHash = wem.Hash,
                            RecursionDepth = recursionDepth,
                        });
                    }
                }
            }
        }

        // Filter out duplicates
        return new ObservableCollection<VoicelineItem>(result.GroupBy(x => x.WemHash.Hash32)
                                                      .Select(group => group.First()));
    }

    private void PlayWem_OnClick(object sender, RoutedEventArgs e)
    {
        VoicelineItem item = (VoicelineItem)(sender as Button).DataContext;
        _activeItem = item;
        MusicPlayer.SetWem(FileResourcer.Get().GetFile<Wem>(item.WemHash));
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

        ConfigSubsystem config = TigerInstance.GetSubsystem<ConfigSubsystem>();
        Wem wem = FileResourcer.Get().GetFile<Wem>(info.Hash);
        string saveDirectory = config.GetExportSavePath() + $"/Sound/Dialogue/{info.Name}/";
        Directory.CreateDirectory(saveDirectory);
        wem.SaveToFile($"{saveDirectory}/{info.Hash}.wav");

        StringBuilder dialogueBuilder = new();
        if (File.Exists($"{saveDirectory}/Dialogue.txt"))
        {
            using (StreamReader reader = new($"{saveDirectory}/Dialogue.txt"))
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

    public FileHash WemHash { get; set; }

    public int RecursionDepth { get; set; }

    public Thickness Padding  // todo make this work nicely
    {
        get => new(Convert.ToDouble(RecursionDepth * 50 - 50), 0, 0, 0);
    }
}
