namespace Tiger.Hashes;


public class StringHash
{
    
}

public interface IHash
{

}


public class SchemaHash : IHash
{
    
}

public class FileHash : IHash
{
    private uint _shortHashValue;

    public FileHash(uint hashValue)
    {
        
    }
}

public class LongFileHash : FileHash
{
    private ulong _longHashValue;

    public LongFileHash(ulong longHashValue) : base(1)
    {
        throw new NotImplementedException();
    }
}

