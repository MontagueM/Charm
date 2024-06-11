using System;
using System.Windows.Controls;
using Tiger;
using Tiger.Schema.Activity.DESTINY2_BEYONDLIGHT_3402;

namespace Charm;

public partial class MusicView : UserControl
{
    public MusicView()
    {
        InitializeComponent();
    }

    public void Load(FileHash fileHash)
    {
        Tag<SMusicTemplate> music = FileResourcer.Get().GetSchemaTag<SMusicTemplate>(fileHash);

        if (music == null || music.TagData.Unk28.Count == 0)
            return;
        //if (music.TagData.Unk28.Count != 1)
        //{
        //    throw new NotImplementedException();
        //}

        var resource = music.TagData.Unk28[0].Unk00.GetValue(music.GetReader());
        if (resource is D2Class_F5458080 f5458080)
        {
            WemsControl.Load(f5458080);
            EventsControl.Load(f5458080);
            FileHash sbhash = null;
            if (Strategy.CurrentStrategy == TigerStrategy.DESTINY2_BEYONDLIGHT_3402)
                sbhash = f5458080.MusicLoopSound.TagData.SoundbankBL.Hash;
            else
                sbhash = f5458080.MusicLoopSound.TagData.Unk18.TagData.SoundBank.Hash;
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
        else if (resource is SUnkMusicE6BF8080 rese6bf)
        {
            EventsControl.Load(rese6bf, music.TagData.MusicTemplateName?.Value);
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
