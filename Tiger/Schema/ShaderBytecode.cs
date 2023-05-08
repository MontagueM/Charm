namespace Tiger.Schema;

public class ShaderBytecode : TigerReferenceFile<SShaderBytecode>
{
    public ShaderBytecode(FileHash hash) : base(hash)
    {
    }

    public byte[] GetBytecode()
    {
        using (TigerReader handle = GetReferenceReader())
        {
            return handle.ReadBytes((int)_tag.BytecodeSize);
        }
    }
}

[SchemaStruct(0x28)]
public struct SShaderBytecode
{
    public ulong FileSize;
    public ulong BytecodeSize;
    public TigerHash Unk0C;
}
