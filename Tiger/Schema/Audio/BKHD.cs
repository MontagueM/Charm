
using System.Text;
using Arithmic;

namespace Tiger.Schema.Audio;

public class BKHD : Tag
{
    public BKHD(FileHash hash) : base(hash)
    {

    }

    // Names aren't stripped from D1 soundbanks
    public string GetNameFromBank()
    {
        try
        {
            using TigerReader reader = GetReader();
            // Get the size of the BKHD chunk
            reader.Seek(0x4, SeekOrigin.Begin);
            var BKHDSize = reader.ReadInt32();

            // Go to the BKHD chunk then get that size
            reader.Seek(BKHDSize + 0x4, SeekOrigin.Current);
            var HIRCSize = reader.ReadInt32();

            // Go to the STID chunk then get the name length
            reader.Seek(HIRCSize + 0x4 + 0x10, SeekOrigin.Current);
            var stringLength = reader.ReadByte();
            return Encoding.UTF8.GetString(reader.ReadBytes(stringLength));
        }
        catch (Exception ex) // Some soundbanks are just empty I guess
        {
            Log.Error(ex.Message);
            return "";
        }

    }
}
