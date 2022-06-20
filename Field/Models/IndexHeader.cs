using System;
using System.Collections.Generic;
using System.IO;
using Field.General;

namespace Field.Models;

public class IndexHeader : Tag
{
    public D2Class_IndexHeader Header;
    public IndexBuffer Buffer;
    
    public IndexHeader(TagHash hash) : base(hash)
    {
    }

    protected override void ParseStructs()
    {
        Header = ReadHeader<D2Class_IndexHeader>();
    }

    protected override void ParseData()
    {
        Buffer = new IndexBuffer(PackageHandler.GetEntryReference(Hash), this);
    }
}