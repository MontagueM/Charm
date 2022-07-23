using Field.General;
using NAudio.Vorbis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Field;

public class WwiseSound : Tag
{
    public D2Class_38978080 Header;
    private MemoryStream _soundStream = null;
    private WaveFileReader _soundReader = null;
    
    public WwiseSound(TagHash hash) : base(hash)
    {
        
    }

    public void Reset()
    {
        _soundStream.Position = 0;
    }

    public void Load()
    {
        var provider = MakeProvider();
        _soundStream = new MemoryStream();
        WaveFileWriter.WriteWavFileToStream(_soundStream, provider.ToWaveProvider());
        _soundStream.Position = 0;
        _soundReader = new WaveFileReader(_soundStream);
    }
    
    protected override void ParseStructs()
    {
        Header = ReadHeader<D2Class_38978080>();
    }
    
    private MixingSampleProvider MakeProvider()
    {
        MixingSampleProvider provider = new MixingSampleProvider(Header.Unk20[0].MakeWaveChannel().WaveFormat);
        foreach (var wem in Header.Unk20)
        {
            provider.AddMixerInput(wem.MakeWaveChannel());
        }
        return provider;
    }
    
    private void CheckLoaded()
    {
        if (_soundReader == null)
            Load();
    }

    public TimeSpan GetDuration()
    {
        CheckLoaded();
        return _soundReader.TotalTime;
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
    
    public WaveChannel32 MakeWaveChannel()
    {
        CheckLoaded();
        var waveChannel = new WaveChannel32(_soundReader);
        waveChannel.PadWithZeroes = false;
        return waveChannel;
    }
}