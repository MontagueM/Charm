using System.Runtime.InteropServices;
using Field.General;
namespace Field;

public class TextureBuffer : Tag
{
    public TextureBuffer(TagHash hash) : base(hash)
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