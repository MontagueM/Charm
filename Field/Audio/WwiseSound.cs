using Field.General;

namespace Field;

public class WwiseSound : Tag
{
    public D2Class_38978080 Header;
    
    public WwiseSound(TagHash hash) : base(hash)
    {
        
    }

    protected override void ParseStructs()
    {
        Header = ReadHeader<D2Class_38978080>();
    }
}