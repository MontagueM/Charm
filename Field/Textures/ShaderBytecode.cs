using Field.General;

namespace Field;

public class ShaderBytecode : Tag
{
    public ShaderBytecode(TagHash hash) : base(hash)
    {
    }

    public byte[] GetBufferData()
    {
        byte[] data;
        using (var handle = GetHandle())
        {
            data = handle.ReadBytes((int)handle.BaseStream.Length);
        }
        return data;
    }
}