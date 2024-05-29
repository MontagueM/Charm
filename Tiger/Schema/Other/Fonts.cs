namespace Tiger.Schema.Other;

// C7478080 shadowkeep
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "DA2F8080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, "0F3C8080", 0x18)]
public struct D2Class_0F3C8080
{
    public long FileSize;
    public DynamicArray<D2Class_113C8080> FontParents;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "F05A8080", 0x04)]
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, "113C8080", 0x04)]
public struct D2Class_113C8080
{
    public Tag<D2Class_123C8080> FontParent;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "D92F8080", 0x20)]
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, "123C8080", 0x20)]
public struct D2Class_123C8080
{
    public long FileSize;
    public TigerFile FontFile;
    [SchemaField(0x10)]
    public StringPointer FontName;
    public long FontFileSize;
}
