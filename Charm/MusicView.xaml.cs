using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Tiger;
using NAudio.Vorbis;
using NAudio.Wave;

namespace Charm;

public partial class MusicView : UserControl
{
    public MusicView()
    {
        InitializeComponent();
    }

    public async void Load(FileHash fileHash)
    {
        Tag<D2Class_EB458080> music = FileResourcer.Get().GetFile<D2Class_EB458080>(fileHash);

        if (music == null)
            return;
        if (music.TagData.Unk28.Count != 1)
        {
            throw new NotImplementedException();
        }

        var resource = music.TagData.Unk28[0].Unk00;
        if (resource is D2Class_F5458080)
        {
            var res = (D2Class_F5458080) resource;
            WemsControl.Load(res);
            EventsControl.Load(res);
            var sbhash = res.MusicLoopSound.TagData.Unk18.TagData.SoundBank.Hash;
            SoundbankHash.Text = $"Soundbank: {sbhash} / {sbhash.GetPkgId().ToString("X4")}-{sbhash.GetEntryIndex().ToString("X4")}";
        }
        else if (resource is D2Class_F7458080)
        {
            var res = (D2Class_F7458080) resource;
            WemsControl.Load(res);
            EventsControl.Load(res);
            if (res.AmbientMusicSet != null)
            {
                var sbhash = res.AmbientMusicSet.TagData.Unk08[0].MusicLoopSound.TagData.Unk18.TagData.SoundBank.Hash;
                SoundbankHash.Text = $"Soundbank: {sbhash} / {sbhash.GetPkgId().ToString("X4")}-{sbhash.GetEntryIndex().ToString("X4")}";
            }
        }
        else
        {
            if (resource is not D2Class_F7458080)
            {
                throw new NotImplementedException();
            }
        }
    }
}
