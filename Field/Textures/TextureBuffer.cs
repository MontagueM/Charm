using System.Runtime.InteropServices;
using Field.General;
namespace Field.Textures;

public class TextureBuffer : Tag
{

    public TextureBuffer(string hash) : base(hash)
    {
    }
    
    public TextureBuffer(TagHash hash) : base(hash)
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