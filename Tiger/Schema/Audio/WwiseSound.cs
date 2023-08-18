using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Tiger.Schema.Audio;

public class WwiseSound : TigerReferenceFile<D2Class_38978080>
{
    private MemoryStream _soundStream;
    private WaveFileReader _soundReader;

    public WwiseSound(FileHash hash) : base(hash)
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

    private MixingSampleProvider MakeProvider()
    {
        MixingSampleProvider provider = new(_tag.Wems[0].MakeWaveChannel()?.WaveFormat);
        Parallel.ForEach(_tag.Wems, wem =>
        {
            if(wem != null)
                provider.AddMixerInput(wem.MakeWaveChannel());
        });

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

    /// <summary>
    /// The playing combines all the audio, the exporting does all as separate.
    /// </summary>
    public void ExportSound(string saveDirectory)
    {
        CheckLoaded();
        _tag.Wems.ForEach(wem =>
        {
            wem.SaveToFile($"{saveDirectory}/{wem.Hash}_{ReferenceHash}.wav");
        });
    }
}
