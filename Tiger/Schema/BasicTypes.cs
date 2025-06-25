namespace Tiger.Schema
{
    [SchemaStruct("04008080", 1)]
    public struct SBool
    {
        public bool Value;
    }

    [SchemaStruct("05008080", 1)]
    public struct SInt8
    {
        public sbyte Value;
    }

    [SchemaStruct("06008080", 2)]
    public struct SInt16
    {
        public short Value;
    }

    [SchemaStruct("07008080", 4)]
    public struct SInt32
    {
        public int Value;
    }

    [SchemaStruct("08008080", 8)]
    public struct SInt64
    {
        public long Value;
    }

    [SchemaStruct("09008080", 1)]
    public struct SUInt8
    {
        public byte Value;
    }

    [SchemaStruct("0A008080", 2)]
    public struct SUInt16
    {
        public ushort Value;
    }

    [SchemaStruct("0B008080", 4)]
    public struct SUInt32
    {
        public uint Value;
    }

    [SchemaStruct("0C008080", 8)]
    public struct SUInt64
    {
        public ulong Value;
    }

    [SchemaStruct("0F008080", 4)]
    public struct SReal32
    {
        public float Value;
    }
}
