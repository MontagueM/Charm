using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Field;
using Field.General;
using NAudio.Vorbis;
using NAudio.Wave;

namespace Charm;

public partial class DialogueView : UserControl
{
    private Dialogue _dialogue;
    private DirectSoundOut _output;
    
    public DialogueView()
    {
        InitializeComponent();
    }

    public void Load(TagHash hash)
    {
        _dialogue = new Dialogue(hash);
        List<dynamic?> result = _dialogue.Load();
        GenerateUI(result);
    }

    private void GenerateUI(List<dynamic?> dialogueTree)
    {
        ListView.ItemsSource = GenerateUIRecursive(0, dialogueTree);
    }

    private ObservableCollection<DisplayVoiceline> GenerateUIRecursive(int recursionDepth, List<dynamic?> dialogueTree)
    {
        ObservableCollection<DisplayVoiceline> result = new ObservableCollection<DisplayVoiceline>();
        foreach (var dyn in dialogueTree)
        {
            if (dyn is List<dynamic?>)
            {
                var a = 0;
                // StackPanel s = new StackPanel();
                ObservableCollection<DisplayVoiceline> res = GenerateUIRecursive(recursionDepth+1, dyn);
                foreach (var q in res)
                {
                    result.Add(q);
                }
            }
            else
            {
                D2Class_33978080 a = dyn;
                // var s = a.Unk28;
                // var q = 0;
                // var t = new TextBlock();
                // t.Text = s;
                result.Add(new DisplayVoiceline() { Narrator = a.NarratorString, Voiceline = a.Unk28, Sound = a.Sound1, RecursionDepth = recursionDepth } );
            }
        }

        return result;
    }

    private void Button_OnClick(object sender, RoutedEventArgs e)
    {
        var q = (sender as Button).Tag as Sound;
        var s = q.Header.Unk20[0].GetWemStream();
        
        _output = new DirectSoundOut(200);
        
        var wc = new WaveChannel32(new VorbisWaveReader(s));
        _output.Init(wc);
        _output.Play();
    }
}

public class DisplayVoiceline
{
    private string narrator;
    private string voiceline;
    private Sound sound;
    private int recursionDepth;
    public string Narrator
    {
        get => narrator;
        set => narrator = value;
    }

    public string PlayString
    {
        get => $"Play {SoundHash}";
    }
    
    public string Voiceline
    {
        get => voiceline;
        set => voiceline = value;
    }

    public string SoundHash
    {
        get => sound.Hash;
    }
    
    public Sound Sound
    {
        get => sound;
        set => sound = value;
    }
    
    public int RecursionDepth
    {
        get => recursionDepth;
        set => recursionDepth = value;
    }

    public Thickness Padding
    {
        get => new Thickness(Convert.ToDouble(recursionDepth*50 - 50), 0, 0, 0);
    }
}