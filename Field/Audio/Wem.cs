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
    
    public Wem(TagHash hash) : base(hash)
    {
    }

    public void Load()
    {
        _wemStream = GetWemStream();
        _wemReader = new VorbisWaveReader(_wemStream);
    }

    private void CheckLoaded()
    {
        if (_wemStream == null)
            Load();
    }
    
    private MemoryStream GetWemStream()
    {
        return WemConverter.ConvertSoundFile(GetStream());
    }
    
    public WaveChannel32 MakeWaveChannel()
    {
        CheckLoaded();
        return new WaveChannel32(_wemReader);
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
}