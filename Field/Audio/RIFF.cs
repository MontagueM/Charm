using Field.General;

namespace Field;

public class RIFF : Tag
{
    public RIFF(TagHash hash) : base(hash)
    {
        
    }

    public MemoryStream GetWemStream()
    {
        return WemConverter.ConvertSoundFile(GetStream());
    }
}