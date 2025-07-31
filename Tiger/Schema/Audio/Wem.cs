using System.Diagnostics;
using Arithmic;
using NAudio.Vorbis;
using NAudio.Wave;
using Tiger.Schema.Audio.ThirdParty;

namespace Tiger.Schema.Audio;

// This has gotten so messy to look at

/// <summary>
/// Used for efficient loading of RIFF tags.
/// Only loads the tag when asked and keeps it cached here, it's ofc still in PackageHandler cache
/// but still a bit more efficient.
/// </summary>
[NonSchemaType(TigerStrategy.DESTINY1_RISE_OF_IRON, 8, new[] { 21 })]
[NonSchemaType(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 26, new[] { 6 })]
[NonSchemaType(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 26, new[] { 7 })]
public class Wem : TigerFile
{
    private MemoryStream _wemStream = null;

    private WaveStream _wemReader = null;
    public WaveStream WemReaderClone = null;

    private bool _bDisposed = false;
    public WEMMetadata? WemData = null;

    public int Channels
    {
        get
        {
            GetWEMData();
            return WemData.Value.Channels;
        }
    }

    public string Duration
    {
        get
        {
            GetWEMData();
            float duration = GetDuration();
            return GetDurationString(duration);
        }
    }

    public float Seconds
    {
        get
        {
            GetWEMData();
            return GetDuration();
        }
    }

    public int SampleRate
    {
        get
        {
            GetWEMData();
            return WemData.Value.SampleRate;
        }
    }

    public Wem(FileHash hash) : base(hash)
    {
    }

    public bool Load()
    {
        if (GetReferenceHash() is null || GetReferenceHash().IsInvalid())
            return false;

        _bDisposed = false;
        if (Strategy.IsLatest()) //TODO Eventually move away from vgmstream, i hope?
        {
            string tempPath = $"{Path.GetTempPath()}/{Hash}.wav";
            VgmstreamConvert(tempPath);

            byte[] wavData = File.ReadAllBytes(tempPath);
            _wemStream = new MemoryStream(wavData);
            _wemReader = new WaveFileReader(_wemStream);
            WemReaderClone = Clone(); //new WaveFileReader(new MemoryStream(wavData));

            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
        else
        {
            _wemStream = GetWemStream();
            _wemReader = new VorbisWaveReader(_wemStream);
            WemReaderClone = Clone();
        }

        try
        {
            if (Channels > 2) // this sucks I hate this so much
                _wemReader = Strategy.IsLatest() ? DownmixToStereoEoF(_wemReader) : DownmixToStereo(_wemReader);
        }
        catch (Exception e)
        {
            Log.Error($"{e.Message}: {_wemReader.WaveFormat.ToString()}");
        }

        return true;
    }

    private void GetWEMData()
    {
        if (WemData is null)
            WemData = GetWEMMetadata(); //WemConverter.GetWwiseRIFFVorbis(GetStream());
    }

    private bool CheckLoaded()
    {
        if (_wemStream == null || _bDisposed)
            return Load();

        return true;
    }

    private MemoryStream GetWemStream()
    {
        // Somethings going on in here maybe that's causing some random artifacting
        // on some audio (especially surround sound)
        return WemConverter.ConvertSoundFile(GetStream());
    }

    public WaveChannel32? MakeWaveChannel()
    {
        CheckLoaded();
        if (_wemReader is null)
            return null;

        try
        {
            var waveChannel = new WaveChannel32(_wemReader);
            waveChannel.PadWithZeroes = false;
            return waveChannel;
        }
        catch (Exception e)
        {
            Log.Error($"{e.Message}: {_wemReader.WaveFormat.ToString()}");
            return null;
        }
    }

    public float GetDuration()
    {
        return (float)WemData.Value.DataSize / (float)WemData.Value.AvgBytesPerSecond;
    }

    public static string GetDurationString(float duration)
    {
        if (duration > 60.0f)
        {
            int minutes = (int)Math.Floor(duration / 60.0f);
            int seconds = (int)(duration % 60.0f);
            return $"{minutes:0}:{seconds:00}";
        }
        else
        {
            int wholeSeconds = (int)duration;
            int milliseconds = (int)(duration * 1000.0f) % 1000;
            return $"{wholeSeconds:0}.{milliseconds:00}s";
        }
    }

    // This is no where near perfect but it's good enough for preview audio...
    public WaveStream DownmixToStereo(WaveStream waveStream)
    {
        WaveFormat inputFormat = waveStream.WaveFormat;
        //if (inputFormat.Channels != 4) // For testing, C8FC1A81 has 5
        //    throw new ArgumentException($"Input stream {Hash} must have 4 channels. Has {waveStream.WaveFormat.Channels}");

        Log.Info($"Downsampling {this.Hash} to Stereo format ({waveStream.WaveFormat.ToString()})");

        var stereoFormat = WaveFormat.CreateIeeeFloatWaveFormat(inputFormat.SampleRate, 2);
        var output = new MemoryStream();
        var writer = new WaveFileWriter(output, stereoFormat);

        int bytesPerSample = inputFormat.BitsPerSample / 8; // 4 bytes for 32-bit IEEE Float
        int frameSize = inputFormat.Channels * bytesPerSample; // Total size of one frame
        byte[] buffer = new byte[frameSize * 1024]; // Buffer size for reading, can be adjusted
        int read;

        while ((read = waveStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            // Ensure we are processing complete frames, adjust read size if necessary
            int numFrames = read / frameSize;

            if (numFrames == 0)
                continue; // Skip if no frames were read (avoid reading partial frames)

            // Convert byte buffer to float samples
            float[] samples = new float[numFrames * inputFormat.Channels];
            for (int i = 0; i < numFrames; i++)
            {
                for (int channel = 0; channel < inputFormat.Channels; channel++)
                {
                    int byteIndex = i * frameSize + channel * bytesPerSample;
                    samples[i * inputFormat.Channels + channel] = BitConverter.ToSingle(buffer, byteIndex);
                }
            }

            // Downmix 4 channels to 2 channels (stereo)
            float[] stereoBuffer = new float[numFrames * 2]; // 2 channels for stereo output
            for (int i = 0, j = 0; i < samples.Length; i += inputFormat.Channels, j += 2)
            {
                // Downmix channels: Combine the 4 channels into left and right stereo
                // In order: Front Left, Front Right, Back Left, Back Right
                // Adding back left and right seem to cause most of the artifacting
                stereoBuffer[j] = Math.Clamp(samples[i], -1f, 1f); // Left 
                stereoBuffer[j + 1] = Math.Clamp(samples[i + 1], -1f, 1f);// Right 
            }

            // Convert the downmixed stereo floats back to bytes
            byte[] stereoBytes = new byte[stereoBuffer.Length * bytesPerSample];
            for (int i = 0; i < stereoBuffer.Length; i++)
            {
                // Convert float back to 32-bit IEEE float for output
                BitConverter.GetBytes(stereoBuffer[i]).CopyTo(stereoBytes, i * bytesPerSample);
            }

            // Write the stereo bytes to the output stream
            writer.Write(stereoBytes, 0, stereoBytes.Length);
        }

        writer.Flush();
        output.Position = 0;
        return new WaveFileReader(output);
    }

    public WaveStream DownmixToStereoEoF(WaveStream input)
    {
        if (input.WaveFormat.Channels <= 2)
            return input; // Already stereo or mono

        ISampleProvider sampleProvider = input.ToSampleProvider();
        var stereoProvider = new StereoDownmixProvider(sampleProvider);
        IWaveProvider waveProvider = stereoProvider.ToWaveProvider16();

        var memoryStream = new MemoryStream();

        using (var writer = new WaveFileWriter(new IgnoreDisposeStream(memoryStream), waveProvider.WaveFormat))
        {
            byte[] buffer = new byte[waveProvider.WaveFormat.AverageBytesPerSecond];
            int bytesRead;
            while ((bytesRead = waveProvider.Read(buffer, 0, buffer.Length)) > 0)
            {
                writer.Write(buffer, 0, bytesRead);
            }
        }

        memoryStream.Position = 0;
        return new WaveFileReader(memoryStream);
    }

    // Slightly faster than getting it from OWSound.WwiseRIFFVorbis. Not by much, but its something
    private WEMMetadata GetWEMMetadata()
    {
        using var reader = GetReader();

        byte[] magic = reader.ReadBytes(4);
        uint dataSize = 0;
        uint channels = 0;
        uint sampleRate = 0;
        uint bytesPerSecond = 0;

        if (magic.SequenceEqual(new byte[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' })) // RIFF = little endian
        {
            reader.Seek(0x4, SeekOrigin.Begin);
            dataSize = reader.ReadUInt32();

            reader.Seek(0x16, SeekOrigin.Begin);
            channels = reader.ReadUInt16();
            sampleRate = reader.ReadUInt32();
            bytesPerSecond = reader.ReadUInt32();
        }
        else if (magic.SequenceEqual(new byte[] { (byte)'R', (byte)'I', (byte)'F', (byte)'X' })) // RIFX = big endian
        {
            reader.Seek(0x4, SeekOrigin.Begin);
            dataSize = Endian.SwapU32(reader.ReadUInt32());

            reader.Seek(0x16, SeekOrigin.Begin);
            channels = Endian.SwapU32(reader.ReadUInt16());
            sampleRate = Endian.SwapU32(reader.ReadUInt32());
            bytesPerSecond = Endian.SwapU32(reader.ReadUInt32());
        }

        return new WEMMetadata
        {
            DataSize = (int)dataSize,
            Channels = (int)channels,
            SampleRate = (int)sampleRate,
            AvgBytesPerSecond = (int)bytesPerSecond
        };
    }

    public WaveStream Clone()
    {
        var memory = new MemoryStream();
        _wemStream.Position = 0;
        _wemStream.CopyTo(memory);
        memory.Position = 0;

        return Strategy.IsLatest() ? new WaveFileReader(memory) : new VorbisWaveReader(memory);
    }

    public void VgmstreamConvert(string savePath)
    {
        string tempPath = Path.Combine(Path.GetTempPath(), $"{Hash}.wem");

        try
        {
            File.WriteAllBytes(tempPath, GetData(false));

            ProcessStartInfo startInfo = new()
            {
                FileName = "ThirdParty/vgmstream/vgmstream-cli.exe",
                Arguments = $"-i -o \"{savePath}\" \"{tempPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            using (Process exeProcess = Process.Start(startInfo))
            {
                exeProcess.WaitForExit();
                if (exeProcess.ExitCode != 0 || !File.Exists(savePath))
                    throw new Exception("Vgmstream conversion failed.");
            }
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    public void SaveToFile(string savePath)
    {
        if (!CheckLoaded())
            return;

        if (Strategy.IsLatest())
        {
            VgmstreamConvert(savePath);
            return;
        }

        _wemReader.Position = 0;

        // Remake the reader so none of the downmix stuff gets exported, though idk if that really matters or not at this point
        if (Channels > 2)
            _wemReader = new VorbisWaveReader(_wemStream);

        // Saves as 16 bit instead of 32 bit, halves file size with no quality loss (afaik)
        WaveFileWriter.CreateWaveFile16(savePath, (ISampleProvider)_wemReader);
    }

    public void Dispose()
    {
        _wemReader?.Dispose();
        _wemStream?.Dispose();
        WemReaderClone?.Dispose();
        _bDisposed = true;
    }

    public struct WEMMetadata
    {
        public int DataSize;
        public int Channels;
        public int SampleRate;
        public int AvgBytesPerSecond;
    }
}

public class StereoDownmixProvider : ISampleProvider
{
    private readonly ISampleProvider source;
    private readonly int sourceChannels;

    public StereoDownmixProvider(ISampleProvider source)
    {
        if (source.WaveFormat.Channels < 2)
            throw new ArgumentException("Source must have at least 2 channels");

        this.source = source;
        this.sourceChannels = source.WaveFormat.Channels;

        WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(
            source.WaveFormat.SampleRate, 2);
    }

    public WaveFormat WaveFormat { get; }

    public int Read(float[] buffer, int offset, int count)
    {
        int sourceCount = count / 2 * sourceChannels;
        float[] sourceBuffer = new float[sourceCount];

        int samplesRead = source.Read(sourceBuffer, 0, sourceCount);
        int framesRead = samplesRead / sourceChannels;

        for (int i = 0; i < framesRead; i++)
        {
            float l = 0f, r = 0f;

            for (int c = 0; c < sourceChannels; c++)
            {
                float sample = sourceBuffer[i * sourceChannels + c];

                // Simple downmix strategy:
                if (c % 2 == 0) l += sample; // even = left
                else r += sample;           // odd = right
            }

            buffer[offset + i * 2] = l / (sourceChannels / 2);
            buffer[offset + i * 2 + 1] = r / (sourceChannels / 2);
        }

        return framesRead * 2;
    }
}

public class IgnoreDisposeStream : Stream
{
    private readonly Stream _inner;

    public IgnoreDisposeStream(Stream inner) => _inner = inner;

    public override bool CanRead => _inner.CanRead;
    public override bool CanSeek => _inner.CanSeek;
    public override bool CanWrite => _inner.CanWrite;
    public override long Length => _inner.Length;
    public override long Position { get => _inner.Position; set => _inner.Position = value; }

    public override void Flush() => _inner.Flush();
    public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
    public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
    public override void SetLength(long value) => _inner.SetLength(value);
    public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);

    // Prevent disposal
    protected override void Dispose(bool disposing) { /* do nothing */ }
}
