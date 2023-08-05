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
using Tiger.Schema;
using Tiger.Schema.Activity;
using Tiger.Schema.Activity.DESTINY2_WITCHQUEEN_6307;

namespace Charm;

public partial class MusicView : UserControl
{
    public MusicView()
    {
        InitializeComponent();
    }

    public async void Load(FileHash fileHash)
    {
        return;
        Tag<D2Class_EB458080> music = FileResourcer.Get().GetSchemaTag<D2Class_EB458080>(fileHash);

        if (music == null)
            return;
        if (music.TagData.Unk28.Count != 1)
        {
            throw new NotImplementedException();
        }

        var resource = music.TagData.Unk28[0].Unk00.GetValue(music.GetReader());
        if (resource is D2Class_F5458080 f5458080)
        {
            WemsControl.Load(f5458080);
            EventsControl.Load(f5458080);
            var sbhash = f5458080.MusicLoopSound.TagData.Unk18.TagData.SoundBank.Hash;
            SoundbankHash.Text = $"Soundbank: {sbhash} / {sbhash.PackageId:X4}-{sbhash.FileIndex:X4}";
        }
        else if (resource is D2Class_F7458080 res)
        {
            WemsControl.Load(res);
            EventsControl.Load(res);
            if (res.AmbientMusicSet != null)
            {
                var sbhash = res.AmbientMusicSet.TagData.Unk08[0].MusicLoopSound.TagData.Unk18.TagData.SoundBank.Hash;
                SoundbankHash.Text = $"Soundbank: {sbhash} / {sbhash.PackageId:X4}-{sbhash.FileIndex:X4}";
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
