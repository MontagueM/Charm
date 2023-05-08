using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Internal.Fbx;
using Microsoft.VisualBasic.FileIO;
using Tiger;

namespace Tiger;

public class Tag<T> : TigerFile where T : struct
{
    protected T _tag;
    // separated as it should be a red flag if we're using this
    public T TagData => _tag;

    // todo verify that T is valid for the hash we get given by checking SchemaStruct against hash reference
    public Tag(FileHash tagHash, bool shouldParse = true) : base(tagHash)
    {
        if (shouldParse)
        {
            Initialise(tagHash);
        }
    }

    public Tag(FileHash tagHash) : base(tagHash)
    {
        Initialise(tagHash);
    }

    private void Initialise(FileHash tagHash)
    {
        if (tagHash.IsValid())
        {
            Deserialize();
        }
        else
        {
            _tag = default;
        }
    }

    private void Deserialize()
    {
        using (TigerReader reader = GetReader())
        {
            _tag = SchemaDeserializer.Get().DeserializeSchema<T>(reader);
        }
    }
}

[SchemaType(0x10)]
public class Tag64<T> : Tag<T> where T : struct
{
    public Tag64(FileHash tagHash, bool shouldParse = true) : base(tagHash, shouldParse) { }
}
