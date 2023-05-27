namespace Tiger;

public static class Endian
{
    public static UInt32 SwapU32(UInt32 value)
    {
        return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
               (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
    }

    public static UInt64 SwapU64(UInt64 value)
    {
        return (0x00000000000000FF) & (value >> 56)
               | (0x000000000000FF00) & (value >> 40)
               | (0x0000000000FF0000) & (value >> 24)
               | (0x00000000FF000000) & (value >> 8)
               | (0x000000FF00000000) & (value << 8)
               | (0x0000FF0000000000) & (value << 24)
               | (0x00FF000000000000) & (value << 40)
               | (0xFF00000000000000) & (value << 56);
    }

    public static string U32ToString(uint number)
    {
        byte[] bytes = BitConverter.GetBytes(number);
        string retval = "";
        foreach (byte b in bytes)
            retval += b.ToString("X2");
        return retval;
    }

    public static string U64ToString(ulong number)
    {
        byte[] bytes = BitConverter.GetBytes(number);
        string retval = "";
        foreach (byte b in bytes)
            retval += b.ToString("X2");
        return retval;
    }
}
