﻿namespace Tiger;

/// <summary>
/// Represents the file behind the Tag, handles all the file-based processing including reading data.
/// To make this thread-safe (particularly the MemoryStream handle) we ensure the byte[] data is cached and only ever
/// read once, then generate a new handle each time GetHandle is called. This should only really ever be used
/// in a dispose-oriented system, as otherwise you will have many leftover streams causing leakage.
/// We store the data in this file instead of in PackageHandler BytesCache as we expect one DestinyFile to be made
/// per tag, never more (as long as PackageHandler GetTag is used).
/// </summary>
public class TigerFile
{
    public FileHash Hash;
    private byte[]? _data = null;
    
    public TigerFile(FileHash hash)
    {
        Hash = hash;
    }
    
    public TigerReader GetReader()
    {
        return new TigerReader(GetStream());
    }

    public MemoryStream GetStream()
    {
        return new MemoryStream(GetData());
    }

    public byte[] GetData()
    {
        if (_data == null)
        {
            _data = PackageResourcer.Get().GetFileData(Hash);
        }

        return _data;
    }
}