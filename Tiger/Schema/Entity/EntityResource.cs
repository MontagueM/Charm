using System.Runtime.InteropServices;

namespace Tiger.Schema.Entity;

public class EntityResource : Tag<D2Class_069B8080>
{
    public D2Class_069B8080 Header;

    protected EntityResource(FileHash hash) : base(hash)
    {
    }
}
