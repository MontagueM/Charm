using Field.General;
using NAudio.Vorbis;
using NAudio.Wave;

namespace Field;

/// <summary>
/// Used for efficient loading of RIFF tags.
/// Only loads the tag when asked and keeps it cached here, it's ofc still in PackageHandler cache
/// but still a bit more efficient.
/// </summary>
public class Wem : Tag
{
    private MemoryStream _wemStream = null;
    private VorbisWaveReader _wemReader = null;
    private bool _bDisposed = false;
    
    public Wem(TagHash hash) : base(hash)
    {
    }

    public void Load()
    {
        _bDisposed = false;
        _wemStream = GetWemStream();
        _wemReader = new VorbisWaveReader(_wemStream);
    }

    private void CheckLoaded()
    {
        if (_wemStream == null || _bDisposed)
            Load();
    }
    
    private MemoryStream GetWemStream()
    {
        return WemConverter.ConvertSoundFile(GetStream());
    }
    
    public WaveChannel32? MakeWaveChannel()
    {
        CheckLoaded();
        try
        {
            var waveChannel = new WaveChannel32(_wemReader);
            waveChannel.PadWithZeroes = false;
            return waveChannel;
        }
        catch (Exception e)
        {
            return null;
        }
    }
    
    public TimeSpan GetDuration()
    {
        CheckLoaded();
        return _wemReader.TotalTime;
    }
    
    public static string GetDurationString(TimeSpan timespan)
    {
        return $"{(int)timespan.TotalMinutes}:{timespan.Seconds:00}";
    }

    public string Duration
    {
        get
        {
            CheckLoaded();
            var timespan = GetDuration();
            return GetDurationString(timespan);
        }
    }

    public void Dispose()
    {
        _wemReader?.Dispose();
        _wemStream?.Dispose();
        _bDisposed = true;
    }

    public void SaveToFile(string savePath)
    {
        CheckLoaded();
        _wemReader.Position = 0;
        WaveFileWriter.CreateWaveFile(savePath, _wemReader);
    }
}