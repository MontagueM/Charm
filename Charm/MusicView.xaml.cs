using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Arithmic;
using Tiger;
using Tiger.Schema.Activity.DESTINY2_BEYONDLIGHT_3402;
using Tiger.Schema.Audio;
using Tiger.Schema.Entity;

namespace Charm;

public partial class MusicView : UserControl
{
    public MusicView()
    {
        InitializeComponent();
    }

    public void Load(FileHash fileHash, dynamic extra = null)
    {
        if (extra is Entity entity)
        {
            List<D2Class_40668080> sounds = new();
            foreach (var resourceHash in entity.TagData.EntityResources.Select(entity.GetReader(), r => r.Resource))
            {
                EntityResource e = FileResourcer.Get().GetFile<EntityResource>(resourceHash);
                if (e.TagData.Unk18.GetValue(e.GetReader()) is D2Class_79818080 a)
                {
                    foreach (var d2ClassF1918080 in a.WwiseSounds1)
                    {
                        if (d2ClassF1918080.Unk10.GetValue(e.GetReader()) is D2Class_40668080 b)
                        {
                            sounds.Add(b);
                        }
                    }
                    foreach (var d2ClassF1918080 in a.WwiseSounds2)
                    {
                        if (d2ClassF1918080.Unk10.GetValue(e.GetReader()) is D2Class_40668080 b)
                        {
                            sounds.Add(b);
                        }
                    }
                }
            }
            WemsControl.Load(sounds);
            return;
        }
        else if (extra is Tag<D2Class_A4BC8080> cine)
        {
            List<WwiseSound> sounds = new();
            if (cine.TagData.Unk08.Count != 0)
                sounds.AddRange(cine.TagData.Unk08.Select(x => x.Sound));

            WemsControl.Load(sounds);
            return;
        }

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
                sbhash = f5458080.MusicLoopSound.TagData.SoundbankWQ.TagData.SoundBank.Hash;
            SoundbankHash.Text = $"Soundbank: {sbhash} / {sbhash.PackageId:X4}-{sbhash.FileIndex:X4}";
        }
        else if (resource is D2Class_F7458080 res)
        {
            WemsControl.Load(res);
            EventsControl.Load(res);
            if (res.AmbientMusicSet != null)
            {
                var sbhash = res.AmbientMusicSet.TagData.Unk08[0].MusicLoopSound.TagData.SoundbankWQ.TagData.SoundBank.Hash;
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
                //throw new NotImplementedException();
                Log.Error($"Music Resource F7458080 Not Implemented");
            }
        }
    }

    // This is bit of a hack since music stuff isnt actually a part of TagListView so gotta jump through some hoops to
    // export fully. (At least I don't think there's a way of doing it right in TagListView?)
    public void Export(ExportInfo info)
    {
        WemsControl.Export();
    }
}
