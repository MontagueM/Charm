﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NAudio.Vorbis;
using NAudio.Wave;
using Tiger;
using Tiger.Schema.Audio;

namespace Charm;

public partial class DialogueView : UserControl
{
    private Dialogue _dialogue;

    public DialogueView()
    {
        InitializeComponent();
    }

    public void Load(FileHash hash)
    {
        _dialogue = new Dialogue(hash);
        List<dynamic?> result = _dialogue.Load();
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
                D2Class_33978080 a = dyn;
                dynamic? unk28 = Strategy.CurrentStrategy == TigerStrategy.DESTINY2_BEYONDLIGHT_3402 ? a.Unk28BL : a.Unk28;
                result.Add(new VoicelineItem
                {
                    Narrator = a.NarratorString,
                    Voiceline = unk28.Value.ToString(),
                    Wem = a.Sound1.TagData.Wems[0],
                    RecursionDepth = recursionDepth,
                    Duration = a.Sound1.TagData.Wems[0].Duration
                });
            }
        }

        return result;
    }

    private void PlayWem_OnClick(object sender, RoutedEventArgs e)
    {
        VoicelineItem item = (VoicelineItem)(sender as Button).DataContext;
        MusicPlayer.SetWem(item.Wem);
        MusicPlayer.Play();
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
