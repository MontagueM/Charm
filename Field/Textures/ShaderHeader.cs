using System.Runtime.InteropServices;
using Field.General;
namespace Field;

public class ShaderHeader : Tag
{
    public D2Class_ShaderHeader Header;

    
    public ShaderHeader(TagHash hash) : base(hash)
    {
    }
    
    protected override void ParseStructs()
    {
        Header = ReadHeader<D2Class_ShaderHeader>();
    }

    public byte[] GetBytecode()
    {
        return new ShaderBytecode(PackageHandler.GetEntryReference(Hash)).GetBufferData();
    }
}

[StructLayout(LayoutKind.Sequential, Size = 0x28)]
public struct D2Class_ShaderHeader
{
    public ulong FileSize;
    public ulong BytecodeSize;
    public DestinyHash Unk0C;
}