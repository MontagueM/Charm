using System.Runtime.InteropServices;
using Field.General;

namespace Field.Entities;

public class EntityResource : Tag
{
    public D2Class_069B8080 Header;
    
    public EntityResource(TagHash hash) : base(hash)
    {
    }

    protected override void ParseStructs()
    {
        Header = ReadHeader<D2Class_069B8080>();
    }

    protected override void ParseData()
    {
    }
}