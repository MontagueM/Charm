using Field.General;

namespace Field.Textures;

public class ShaderBytecode : Tag
{
    public ShaderBytecode(TagHash hash) : base(hash)
    {
    }

    public byte[] GetBufferData()
    {
        GetHandle();
        byte[] data = Handle.ReadBytes((int)Handle.BaseStream.Length);
        CloseHandle();
        return data;
    }
}