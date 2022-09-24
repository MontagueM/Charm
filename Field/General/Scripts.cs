using System.Runtime.InteropServices;
using System.Text;
using Field.General;
using Field.Models;

namespace Field;

/// <summary>
/// This summary is for type 12928080
/// ----
/// B8438080 represents an if statement (can be an else if)
///     Unk18 is the condition
///     Unk30 is the result if true
/// B7438080 is
/// BE438080 is some kind of setter
/// 14928080 represents a block of code that does "something"
/// C2438080 is an assignment/update/increment?
/// D5428080 is a condition for a statement
/// 
/// </summary>
public class Script : Tag
{
    public D2Class_12928080 Header;

    public Script(TagHash hash) : base(hash)
    {
    }

    protected override void ParseStructs()
    {
        Header = ReadHeader<D2Class_12928080>();
    }

    public string ConvertToString()
    {
        var sb = new StringBuilder();
        
        foreach (var script in Header.Unk08)
        {
            sb.AppendLine($"Script {script.Unk00}:");
            Decompile(script.Unk08, sb, 1);
        }
        
        return sb.ToString();
    }

    private void Decompile(dynamic? resource, StringBuilder sb, int indentation)
    {
        if (resource is D2Class_B8438080 rb8)
        {
            D2Class_B7918080 condition;
            if (rb8.Unk18.Count != 1)
            {
                sb.AppendLine("Unknown condition count for D2Class_B8438080, trying anyway");
                condition = rb8.Unk18[1];
            }
            else
                condition = rb8.Unk18[0];
            if (condition.Unk08 is D2Class_D5428080 condd5)
                AppendLine(sb, indentation, $"if ({condd5.Unk00}):");
            else if (condition.Unk08 is D2Class_CD428080 condcd)
                AppendLine(sb, indentation, $"if (UNK[{condcd.Unk00}]):");
            else if (condition.Unk08 is D2Class_D3428080 condd3)
                AppendLine(sb, indentation, $"if (UNK[{condd3.Unk00}]):");
            else
                throw new NotImplementedException();
            
            foreach (var entry in rb8.Unk30)
            {
                Decompile(entry.Unk08, sb, indentation + 1);
            }
        }
        else if (resource is D2Class_C2438080 rc2)
        {
            AppendLine(sb, indentation, $"{rc2.Unk00} = {rc2.Unk08};");
        }
        else if (resource is D2Class_BE438080 rbe)  // multi-line if statement/if else/if else if?
        {
            foreach (var entry in rbe.Unk10)
            {
                Decompile(entry.Unk08, sb, indentation);
            }
        }
        else if (resource is D2Class_B6438080 rb6)
        {
            AppendLine(sb, indentation, $"[B6438080]{rb6.Unk00};");
        }
        else if (resource is D2Class_C3438080 rc3)
        {
            AppendLine(sb, indentation, $"Link->{rc3.Entity.Hash}[{rc3.TagName}];");
        }
        else if (resource is D2Class_A9438080 ra9)
        {
            AppendLine(sb, indentation, $"[A9438080]{ra9.Unk80};");
        }
        else if (resource is D2Class_A6438080 ra6)
        {
            AppendLine(sb, indentation, $"[A6438080]{ra6.Unk00};");
        }
        else if (resource is D2Class_80438080 r80)
        {
            AppendLine(sb, indentation, $"[80438080]{r80.Unk00};");
        }
        else if (resource is D2Class_B7438080 rb7)
        {
            foreach (var entry in rb7.Unk10)
            {
                Decompile(entry.Unk08, sb, indentation + 1);
            }
        }
        else if (resource is D2Class_BC438080 rbc)
        {
            AppendLine(sb, indentation, $"[BC438080]{rbc.Unk08};");
        }
        else if (resource is D2Class_48218080 r48)
        {
            AppendLine(sb, indentation, $"[48218080]{r48.Unk08};");
        }
        else if (resource is D2Class_97438080 r97)
        {
            AppendLine(sb, indentation, $"[48218080]{r97.Unk08};");
            foreach (var entry in r97.Unk18)
            {
                Decompile(entry.Unk08, sb, indentation + 1);
            }
        }
        else
        {
            throw new NotImplementedException();
        }
        var a = 0;
    }
    
    private void AppendLine(StringBuilder sb, int indentation, string line)
    {
        sb.AppendLine(new string('\t', indentation * 2) + line);
    }
}

[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_12928080
{
    public long FileSize;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_14928080> Unk08;
}


[StructLayout(LayoutKind.Sequential, Size = 0x10)]
public struct D2Class_14928080
{
    public DestinyHash Unk00;
    [DestinyOffset(0x8), DestinyField(FieldType.ResourcePointer)]
    public dynamic? Unk08;  // b8438080
}

[StructLayout(LayoutKind.Sequential, Size = 0x68)]
public struct D2Class_B8438080
{
    public long Unk00;
    public long Unk08;
    public long Unk10;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_B7918080> Unk18;
    public long Unk28;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_14928080> Unk30;
    public long Unk40;
    public long Unk48;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_14928080> Unk50;
}

[StructLayout(LayoutKind.Sequential, Size = 0x10)]
public struct D2Class_B7918080
{
    public long Unk00;
    [DestinyOffset(0x8), DestinyField(FieldType.ResourcePointer)]
    public dynamic? Unk08;  // c8428080, d5428080
}

[StructLayout(LayoutKind.Sequential, Size = 8)]
public struct D2Class_C8428080
{
    public DestinyHash Unk00;
}

[StructLayout(LayoutKind.Sequential, Size = 0x90)]
public struct D2Class_D5428080
{
    [DestinyField(FieldType.RelativePointer)]
    public string Unk00;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_0F928080> Unk08;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_09008080> Unk18;
    public long Unk28;
    public long Unk30;
    public long Unk38;
    public long Unk40;
    public long Unk48;
    public long Unk50;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_09008080> Unk58;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_90008080> Unk68;
    public long Unk78;
    public long Unk80;
    public long Unk88;
}

[StructLayout(LayoutKind.Sequential, Size = 8)]
public struct D2Class_0F928080
{
    public int Unk00;
    public DestinyHash Unk08;
}


[StructLayout(LayoutKind.Sequential, Size = 4)]
public struct D2Class_CD428080
{
    public DestinyHash Unk00;
}

[StructLayout(LayoutKind.Sequential, Size = 0x50)]
public struct D2Class_C2438080
{
    public DestinyHash Unk00;
    [DestinyOffset(0x8), DestinyField(FieldType.RelativePointer)]
    public string Unk08;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_0F928080> Unk10;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_09008080> Unk20;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_90008080> Unk30;
    public long Unk40;
    public long Unk48;
}

[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct D2Class_BE438080
{
    public long Unk00;
    public long Unk08;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_14928080> Unk10;
}

[StructLayout(LayoutKind.Sequential, Size = 4)]
public struct D2Class_B6438080
{
    public DestinyHash Unk00;
}

[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_C3438080
{
    [DestinyField(FieldType.RelativePointer)]
    public string TagName;
    [DestinyField(FieldType.TagHash64)] 
    public Tag Entity;  // usually a hopon
}

[StructLayout(LayoutKind.Sequential, Size = 0xC0)]
public struct D2Class_A9438080
{
    [DestinyOffset(0x68)]
    public long Unk68;
    [DestinyOffset(0x80)]
    public DestinyHash Unk80;
    [DestinyOffset(0xB0)]
    public int UnkB0;
    public float UnkB4;
    public long UnkB8;
}

[StructLayout(LayoutKind.Sequential, Size = 8)]
public struct D2Class_A6438080
{
    public DestinyHash Unk00;
    public TagHash Unk04;
}

[StructLayout(LayoutKind.Sequential, Size = 0x58)]
public struct D2Class_80438080
{
    public DestinyHash Unk00;
    [DestinyOffset(0x50)]
    public long Unk50;
}

[StructLayout(LayoutKind.Sequential, Size = 0x28)]
public struct D2Class_B7438080
{
    public long Unk00;
    public long Unk08;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_14928080> Unk10;
}

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct D2Class_D3428080
{
    public byte Unk00;
}

[StructLayout(LayoutKind.Sequential, Size = 0x50)]
public struct D2Class_BC438080
{
    public DestinyHash Unk00;
    [DestinyOffset(0x8), DestinyField(FieldType.RelativePointer)]
    public string Unk08;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_0F928080> Unk10;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_09008080> Unk20;
    public long Unk30;
    public long Unk38;
    public long Unk40;
    public long Unk48;
}

[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_48218080
{
    public long Unk00;
    public long Unk08;
    public long Unk10;
}

[StructLayout(LayoutKind.Sequential, Size = 0x30)]
public struct D2Class_97438080
{
    [DestinyField(FieldType.ResourcePointer)]
    public dynamic? Unk00; // 99438080
    public long Unk08;
    public long Unk10;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_14928080> Unk18;
}

[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct D2Class_99438080
{
    public Vector4 Unk00;
    public Vector4 Unk10;
}

public class ScriptBS : Tag
{
    public D2Class_00008080 Header;

    public ScriptBS(TagHash hash) : base(hash)
    {
    }

    protected override void ParseStructs()
    {
        Header = ReadHeader<D2Class_00008080>();
    }
}

[StructLayout(LayoutKind.Sequential, Size = 0x58)]
public struct D2Class_00008080
{
    public long FileSize;
    public DestinyHash Unk08;
    [DestinyOffset(0x10), DestinyField(FieldType.TagHash)] 
    public Tag Unk10;
    public uint Unk14;
    public byte Unk18;
    public byte Unk19;
    public short Unk1A;
    public short Unk1C;
    public byte Unk1E;
    public byte Unk1F;
    public uint Unk20;
    [DestinyOffset(0x28), DestinyField(FieldType.ResourcePointer)]
    public dynamic? Unk28;
    [DestinyField(FieldType.ResourcePointer)]
    public dynamic? Unk30;
    [DestinyField(FieldType.ResourcePointer)]
    public dynamic? Unk38;
    [DestinyField(FieldType.ResourcePointer)]
    public dynamic? Unk40;
    [DestinyField(FieldType.ResourcePointer)]
    public dynamic? Unk48;
    [DestinyField(FieldType.ResourcePointer)]
    public dynamic? Unk50;
}