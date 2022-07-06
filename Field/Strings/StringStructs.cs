using System.Runtime.InteropServices;
using Field.General;
using Field.Strings;

namespace Field;

[StructLayout(LayoutKind.Sequential, Size = 0x50)]
public struct D2Class_EF998080
{
    public long FileSize;
    [DestinyField(FieldType.TablePointer)]
    public List<DestinyHash> StringHashTable;
    // [DestinyField(FieldType.TagHash), MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
    // public StringData[] StringData;
    [DestinyField(FieldType.TagHash)]  // only working with english rn
    public StringData StringData;
}

[StructLayout(LayoutKind.Sequential, Size = 0x4)]
public struct D2Class_70008080
{
    public DestinyHash StringHash;
}
    
[StructLayout(LayoutKind.Sequential, Size = 0x58)]
public struct D2Class_F1998080
{
    public long FileSize;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_F7998080> StringParts;
    // might be a colour table here
    [DestinyOffset(0x28), DestinyField(FieldType.TablePointer)]
    public List<D2Class_05008080> StringData;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_F5998080> StringCombinations;
}

[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct D2Class_F7998080
{
    [DestinyOffset(0x8), DestinyField(FieldType.RelativePointer)]
    public long StringDataPointer;
    public DestinyHash Unk10;
    public ushort ByteLength;  // these can differ if multibyte unicode
    public ushort StringLength;
    // public ushort CipherShift;  // now always zero
}
    
[StructLayout(LayoutKind.Sequential, Size = 0x1)]
public struct D2Class_05008080
{
    public byte StringCharacter;
}

[StructLayout(LayoutKind.Sequential, Size = 0x10)]
public struct D2Class_F5998080
{
    [DestinyField(FieldType.RelativePointer)]
    public long StartStringPartPointer;
    public long PartCount;
}
    
    
// no-container string caching

[StructLayout(LayoutKind.Sequential, Size = 0x68)]
public struct D2Class_02218080
{
    public long FileSize;
    // public long Unk08;
    // [DestinyField(FieldType.TagHash)] 
    // public Tag Unk10; // 0D3C8080
    // [DestinyField(FieldType.TagHash)] 
    // public Tag Unk14;
    // [DestinyField(FieldType.TablePointer)]
    // public List<D2Class_133C8080> Unk18;
    [DestinyOffset(0x28), DestinyField(FieldType.TablePointer)]
    public List<D2Class_0E3C8080> Unk28;
    // [DestinyField(FieldType.TagHash)] 
    // public Tag Unk38;
    // [DestinyField(FieldType.TagHash)] 
    // public Tag Unk3C;
    // [DestinyField(FieldType.TagHash)] 
    // public Tag Unk40;
    // [DestinyField(FieldType.TagHash)] 
    // public Tag Unk44;
    // [DestinyField(FieldType.TagHash)] 
    // public Tag Unk48;
    // [DestinyField(FieldType.TagHash)] 
    // public Tag Unk4C;
    // [DestinyField(FieldType.TagHash)] 
    // public Tag Unk50;
    // [DestinyOffset(0x58), DestinyField(FieldType.TablePointer)]
    // public List<D2Class_04218080> Unk58;
}
    
[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_133C8080
{
    public DestinyHash Unk00;
    [DestinyOffset(0x8), DestinyField(FieldType.TablePointer)]
    private List<D2Class_153C8080> Unk08;
}
    
[StructLayout(LayoutKind.Sequential, Size = 8)]
public struct D2Class_153C8080
{
    public DestinyHash Unk00;
    public Tag Unk08;
}
    
[StructLayout(LayoutKind.Sequential, Size = 0x28)]
public struct D2Class_0E3C8080
{
    [DestinyField(FieldType.TagHash64)]
    public Tag Unk00;
    [DestinyField(FieldType.TagHash64)]
    public StringContainer Unk10;  // this can be a string container
}
    
[StructLayout(LayoutKind.Sequential, Size = 0x50)]
public struct D2Class_04218080
{
    // this causes a duplication of the entire table, dunno why it does this but i wont bother
}