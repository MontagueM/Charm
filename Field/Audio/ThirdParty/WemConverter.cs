﻿using DataTool.ConvertLogic;

namespace Field;

// DataTool.ConvertLogic.Sound class: https://github.com/overtools/OWLib/blob/develop/DataTool/ConvertLogic/OWSound.cs
// Modified RevorbStd: https://github.com/xyx0826/revorbstd/tree/big-enough
// Modified librevorb.dll (RevorbStd dependency): https://github.com/xyx0826/librevorb/releases/tag/v0.5

using DataTool.ConvertLogic;
using RevorbStd;
using System.IO;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// <para>Converts a Wwise Encoded Media (.wem) to an Ogg Vorbis file.</para>
/// <para>Copyright (c) 2019 overtools</para>
/// <para>Thanks: zingballyhoo and naomi</para>
/// </summary>
static class WemConverter
{
    private const string CodebookPath = "packed_codebooks_aoTuV_603.bin";

    private static bool _fileChecked;

    public static MemoryStream ConvertSoundFile(Stream stream)
    {
        if (!_fileChecked)
        {
            CheckCodebookFile();
        }

        using OWSound.WwiseRIFFVorbis vorbis = new OWSound.WwiseRIFFVorbis(stream, CodebookPath);
        var vorbisStream = new MemoryStream();
        vorbis.ConvertToOgg(vorbisStream);
        vorbisStream.Position = 0;
        return Revorb.Jiggle(vorbisStream.ToArray());
    }

    private static void CheckCodebookFile()
    {
        if (!(_fileChecked = File.Exists(CodebookPath)))
        {
            throw new FileNotFoundException($"WEM conversion codebook {CodebookPath} is missing.");
        }
    }
}