namespace Tiger.Schema;

[SchemaStruct("5B698080", 0x70)]
public struct UnkLights
{
    public long ThisSize;
    public DynamicArray<Unk63698080> Unk0x08;
    public DynamicArray<Unk64698080> Unk0x18;
    public FileHash Unk0x28;
    public FileHash Unk0x2C;
    public ushort Unk0x30;
    public ushort Unk0x32;
    public ushort Unk0x34;
    public ushort Unk0x36;
    public FileHash Unk0x38;
    [SchemaField(0x40)]
    public Vector4 Unk0x40;
    public Vector4 Unk0x50;
}

[SchemaStruct("63698080", 0x8)]
public struct Unk63698080
{
    public FileHash Unk00;
    public ushort StartIndex;
    public ushort Count;
}

[SchemaStruct("64698080", 0x10)]
public struct Unk64698080
{
    public Vector4 Translation;
}
