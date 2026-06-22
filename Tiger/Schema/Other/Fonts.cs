namespace Tiger.Schema.Other;

// C7478080 shadowkeep
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802FDA, 0x18)] //DA2F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80803C0F, 0x18)] //0F3C8080
public struct S80803C0F
{
    public long FileSize;
    public DynamicArray<S80803C11> FontParents;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80805AF0, 0x04)] //F05A8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80803C11, 0x04)] //113C8080
public struct S80803C11
{
    public Tag<S80803C12> FontParent;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802FD9, 0x20)] //D92F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80803C12, 0x20)] //123C8080
public struct S80803C12
{
    public long FileSize;
    public TigerFile FontFile;
    [SchemaField(0x10)]
    public StringPointer FontName;
    public long FontFileSize;
}
