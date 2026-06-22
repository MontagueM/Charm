using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Arithmic;
using Tiger;
using Tiger.Schema.Activity.DESTINY2_BEYONDLIGHT_3402;
using Tiger.Schema.Activity.DESTINY2_SHADOWKEEP_2601;
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
        if (Strategy.IsPreBL())
        {
            LoadPreBL(fileHash);
            return;
        }

        if (extra is Entity entity)
        {
            List<SSequenceAudioEvent> sounds = new();
            foreach (FileHash? resourceHash in entity.Components)
            {
                EntityComponent e = FileResourcer.Get().GetFile<EntityComponent>(resourceHash);
                if (e.TagData.Unk18.GetValue(e.GetReader()) is S80808179 a)
                {
                    foreach (S808091F1 d2ClassF1918080 in a.Array1)
                    {
                        if (d2ClassF1918080.Unk10.GetValue(e.GetReader()) is SSequenceAudioEvent b)
                        {
                            sounds.Add(b);
                        }
                    }
                    foreach (S808091F1 d2ClassF1918080 in a.Array2)
                    {
                        if (d2ClassF1918080.Unk10.GetValue(e.GetReader()) is SSequenceAudioEvent b)
                        {
                            sounds.Add(b);
                        }
                    }
                }
            }
            WemsControl.Load(sounds);
            return;
        }
        else if (extra is Tag<S8080BCA4> cine)
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

        dynamic? resource = music.TagData.Unk28[0].Unk00.GetValue(music.GetReader());
        if (resource is S808045F5 f5458080)
        {
            WemsControl.Load(f5458080);
            EventsControl.Load(f5458080);
            FileHash sbhash = f5458080.MusicLoopSound.TagData.GetSoundbank().Hash;
            SoundbankHash.Text = $"Soundbank: {sbhash} / {sbhash.PackageId:X4}-{sbhash.FileIndex:X4}";
        }
        else if (resource is S808045F7 res)
        {
            WemsControl.Load(res);
            EventsControl.Load(res);
            if (res.AmbientMusicSet != null)
            {
                FileHash sbhash = res.AmbientMusicSet.TagData.Unk08[0].MusicLoopSound.TagData.GetSoundbank().Hash;
                SoundbankHash.Text = $"Soundbank: {sbhash} / {sbhash.PackageId:X4}-{sbhash.FileIndex:X4}";
            }
        }
        else if (resource is SUnkMusicE6BF8080 rese6bf)
        {
            EventsControl.Load(rese6bf, music.TagData.MusicTemplateName?.Value);
        }
        else
        {
            if (resource is not S808045F7)
            {
                //throw new NotImplementedException();
                Log.Error($"Music Resource F7458080 Not Implemented");
            }
        }
    }

    public void LoadPreBL(FileHash hash)
    {
        List<WwiseSound> sounds = new();
        EntityComponent resource = FileResourcer.Get().GetFile<EntityComponent>(hash);
        foreach (dynamic? value in ((S80804E8F)resource.TagData.Unk18.GetValue(resource.GetReader())).Pointers.Select(x => x.Pointer.GetValue(resource.GetReader())))
        {
            switch (value)
            {
                case S80804E95 entry1:
                    if (entry1.Sound is not null)
                        sounds.Add(entry1.Sound);
                    break;
                case S80804E94 entry2:
                    if (entry2.Unk00 is not null)
                    {
                        foreach (S80808E5A sound in entry2.Unk00.TagData.Unk08)
                        {
                            if (sound.Sound is not null)
                                sounds.Add(sound.Sound);
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException($"{hash}");
            }
        }

        WemsControl.Load(sounds.DistinctBy(x => x.Hash).ToList());
        //FileHash sbhash = sound.TagData.SoundbankBL.Hash;
        //SoundbankHash.Text = $"Soundbank: {sbhash} / {sbhash.PackageId:X4}-{sbhash.FileIndex:X4}";
    }

    // This is bit of a hack since music stuff isnt actually a part of TagListView so gotta jump through some hoops to
    // export fully. (At least I don't think there's a way of doing it right in TagListView?)
    public void Export(ExportInfo info)
    {
        WemsControl.Export();
    }
}
