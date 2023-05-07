using System;
using System.Collections.Generic;
using System.IO;

namespace Tiger.Schema;

// public class VertexHeader : Tag
// {
//     public D2Class_VertexHeader Header;
//     public VertexBuffer Buffer;
//
//     public VertexHeader(TagHash hash) : base(hash)
//     {
//     }
//
//     protected override void ParseStructs()
//     {
//         Header = ReadHeader<D2Class_VertexHeader>();
//     }
//
//     protected override void ParseData()
//     {
//         Buffer = new VertexBuffer(TagHash.GetEntryReference(Hash), this);
//     }
// }
