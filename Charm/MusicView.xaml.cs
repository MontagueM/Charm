using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Field;
using Field.General;
using NAudio.Vorbis;
using NAudio.Wave;

namespace Charm;

public partial class MusicView : UserControl
{
    public MusicView()
    {
        InitializeComponent();
    }

    public async void Load(TagHash tagHash)
    {
        Tag<D2Class_EB458080> music = PackageHandler.GetTag<D2Class_EB458080>(tagHash);

        if (music == null)
            return;
        if (music.Header.Unk28.Count != 1)
        {
            throw new NotImplementedException();
        }

        var resource = music.Header.Unk28[0].Unk00;
        if (resource is D2Class_F5458080)
        {
            var res = (D2Class_F5458080) resource;
            WemsControl.Load(res);
            EventsControl.Load(res);
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