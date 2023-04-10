using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Field.Entities;
using Field.General;
using Field.Models;
using File = System.IO.File;

namespace Field;

public class Material : Tag
{
    public D2Class_AA6D8080 Header;
    public static object _lock = new object();

    
    public Material(TagHash hash) : base(hash)
    {
    }

    protected override void ParseStructs()
    {
        Header = ReadHeader<D2Class_AA6D8080>();
    }

    public void SaveAllTextures(string saveDirectory)
    {
        foreach (var e in Header.VSTextures)
        {
            if (e.Texture == null)
            {
                continue;
            }
            // todo change to 64 bit hash?
            string path = $"{saveDirectory}/VS_{e.TextureIndex}_{e.Texture.Hash}";
            if (!File.Exists(path))
            {
                e.Texture.SavetoFile(path); 
            }
        }
        foreach (var e in Header.PSTextures)
        {
            if (e.Texture == null)
            {
                continue;
            }
            // todo change to 64 bit hash?
            string path = $"{saveDirectory}/{e.Texture.Hash}";
            if (!File.Exists(path + ".dds") && !File.Exists(path + ".png") && !File.Exists(path + ".tga"))
            {
                e.Texture.SavetoFile(path); 
            }
        }
    }
    
    // [DllImport("HLSLDecompiler.dll", EntryPoint = "DecompileHLSL", CallingConvention = CallingConvention.Cdecl)]
    // public static extern IntPtr DecompileHLSL(
    //     IntPtr pShaderBytecode,
    //     int BytecodeLength,
    //     out int pHlslTextLength
    // );

    public string Decompile(byte[] shaderBytecode, string? type = "ps")
    {
        // tried doing it via dll pinvoke but seemed to cause way too many problems so doing it via exe instead
        // string hlsl;
        // lock (_lock)
        // {
        //     GCHandle gcHandle = GCHandle.Alloc(shaderBytecode, GCHandleType.Pinned);
        //     IntPtr pShaderBytecode = gcHandle.AddrOfPinnedObject();
        //     IntPtr pHlslText = Marshal.AllocHGlobal(5000);
        //     int len;
        //     pHlslText = DecompileHLSL(pShaderBytecode, shaderBytecode.Length, out int pHlslTextLength);
        //     // len = Marshal.ReadInt32(pHlslTextLength);
        //     len = pHlslTextLength;
        //     hlsl = Marshal.PtrToStringUTF8(pHlslText);
        //     gcHandle.Free();
        // }
        // // Marshal.FreeHGlobal(pHlslText);
        // return hlsl;
    
        string directory = "hlsl_temp";
        string binPath = $"{directory}/{type}{Hash}.bin";
        string hlslPath = $"{directory}/{type}{Hash}.hlsl";

      

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory("hlsl_temp/");
        }

        lock (_lock)
        {
            if (!File.Exists(binPath))
            {
                File.WriteAllBytes(binPath, shaderBytecode);
            } 
        }

        if (!File.Exists(hlslPath))
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "3dmigoto_shader_decomp.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = $"-D {binPath}";
            using (Process exeProcess = Process.Start(startInfo))
            {
                exeProcess.WaitForExit();
            }

            if (!File.Exists(hlslPath))
            {
                throw new FileNotFoundException($"Decompilation failed for {Hash}");
            }
        }

        string hlsl = "";
        lock (_lock)
        {
            while (hlsl == "")
            {
                try  // needed for slow machines
                {
                    hlsl = File.ReadAllText(hlslPath);
                }
                catch (IOException)
                {
                    Thread.Sleep(100);
                }
            }
        }
        return hlsl;
    }
    
    public void SavePixelShader(string saveDirectory, bool isTerrain = false, bool saveCBuffers = false)
    {
        if (Header.PixelShader != null)
        {
            string hlsl = Decompile(Header.PixelShader.GetBytecode());
            string usf = FieldConfigHandler.GetUnrealInteropEnabled() ? new UsfConverter().HlslToUsf(this, hlsl, false) : "";
            string vfx = Source2Handler.source2Shaders ? new VfxConverter().HlslToVfx(this, hlsl, false, isTerrain) : "";


			if (Source2Handler.source2Shaders)
            {
				Directory.CreateDirectory($"{saveDirectory}/Source2");
				Directory.CreateDirectory($"{saveDirectory}/Source2/materials");   
			}

			if (saveCBuffers)
                SaveCbuffers(this, false, hlsl, saveDirectory);

            try
            {
                if(usf != String.Empty && !File.Exists($"{saveDirectory}/PS_{Hash}.usf"))
                {
                    File.WriteAllText($"{saveDirectory}/PS_{Hash}.usf", usf);
                }
                if (vfx != String.Empty && !File.Exists($"{saveDirectory}/Source2/PS_{Hash}.shader"))
                {
                    File.WriteAllText($"{saveDirectory}/Source2/PS_{Hash}.shader", vfx);
                }
            }
            catch (IOException)  // threading error
            {
            }
            
            //Need to save material after shader has be exported, to check if it exists
			if (Source2Handler.source2Shaders)
				Source2Handler.SaveVMAT(saveDirectory, Hash, Header);
		}
    }
    
    public void SaveVertexShader(string saveDirectory, bool saveCBuffers = false)
    {
        Directory.CreateDirectory($"{saveDirectory}");
        if (Header.VertexShader != null)
        {
            string hlsl = Decompile(Header.VertexShader.GetBytecode(), "vs");

            if (saveCBuffers)
                SaveCbuffers(this, true, hlsl, saveDirectory);
            //string usf = new UsfConverter().HlslToUsf(this, hlsl, true);
            //if (usf != String.Empty)
            //{
            //    try
            //    {
            //        File.WriteAllText($"{saveDirectory}/VS_{Hash}.usf", usf);
            //        Console.WriteLine($"Saved vertex shader {Hash}");
            //    }
            //    catch (IOException)  // threading error
            //    {
            //    }
            //}
        }
    }

    public void SaveComputeShader(string saveDirectory)
    {
        Directory.CreateDirectory($"{saveDirectory}");
        if (Header.ComputeShader != null && !File.Exists($"{saveDirectory}/CS_{Hash}.usf"))
        {
            string hlsl = Decompile(Header.ComputeShader.GetBytecode(), "cs");
            string usf = new UsfConverter().HlslToUsf(this, hlsl, false);
            if (usf != String.Empty)
            {
                try
                {
                    File.WriteAllText($"{saveDirectory}/CS_{Hash}.usf", usf);
                    Console.WriteLine($"Saved compute shader {Hash}");
                }
                catch (IOException)  // threading error
                {
                }
            }
        }
    }

    private void SaveCbuffers(Material material, bool bIsVertexShader, string hlsltest, string saveDirectory)
    {
        Directory.CreateDirectory($"{saveDirectory}/CBuffers");

        List<Cbuffer> cbuffers = new List<Cbuffer>();
        StringBuilder buffers = new StringBuilder();
        StringReader hlsl;
        hlsl = new StringReader(hlsltest);

        string line = string.Empty;
        do
        {
            line = hlsl.ReadLine();
            if (line != null)
            {
                if (line.Contains("cbuffer"))
                {
                    hlsl.ReadLine();
                    line = hlsl.ReadLine();
                    Cbuffer cbuffer = new Cbuffer();
                    cbuffer.Variable = "cb" + line.Split("cb")[1].Split("[")[0];
                    cbuffer.Index = Int32.TryParse(new string(cbuffer.Variable.Skip(2).ToArray()), out int index) ? index : -1;
                    cbuffer.Count = Int32.TryParse(new string(line.Split("[")[1].Split("]")[0]), out int count) ? count : -1;
                    cbuffer.Type = line.Split("cb")[0].Trim();
                    cbuffers.Add(cbuffer);
                }
            }

        } while (line != null);

        foreach (var cbuffer in cbuffers)
        {
            buffers.AppendLine($"static {cbuffer.Type} {cbuffer.Variable}[{cbuffer.Count}] = ").AppendLine("{");

            dynamic data = null;
            if (bIsVertexShader)
            {
                if (cbuffer.Count == material.Header.Unk90.Count)
                {
                    data = material.Header.Unk90;
                }
                else if (cbuffer.Count == material.Header.UnkA0.Count)
                {
                    data = material.Header.UnkA0;
                }
                else if (cbuffer.Count == material.Header.UnkB0.Count)
                {
                    data = material.Header.UnkB0;
                }
                else if (cbuffer.Count == material.Header.UnkC0.Count)
                {
                    data = material.Header.UnkC0;
                }
            }
            else
            {
                if (cbuffer.Count == material.Header.Unk2D0.Count)
                {
                    data = material.Header.Unk2D0;
                }
                else if (cbuffer.Count == material.Header.Unk2E0.Count)
                {
                    data = material.Header.Unk2E0;
                }
                else if (cbuffer.Count == material.Header.Unk2F0.Count)
                {
                    data = material.Header.Unk2F0;
                }
                else if (cbuffer.Count == material.Header.Unk300.Count)
                {
                    data = material.Header.Unk300;
                }
                else
                {
                    if (material.Header.PSVector4Container.Hash != 0xffff_ffff)
                    {
                        // Try the Vector4 storage file
                        DestinyFile container = new DestinyFile(PackageHandler.GetEntryReference(material.Header.PSVector4Container));
                        byte[] containerData = container.GetData();
                        int num = containerData.Length / 16;
                        if (cbuffer.Count == num)
                        {
                            List<Vector4> float4s = new List<Vector4>();
                            for (int i = 0; i < containerData.Length / 16; i++)
                            {
                                float4s.Add(StructConverter.ToStructure<Vector4>(containerData.Skip(i * 16).Take(16).ToArray()));
                            }

                            data = float4s;
                        }
                    }

                }
            }


            for (int i = 0; i < cbuffer.Count; i++)
            {
                switch (cbuffer.Type)
                {
                    case "float4":
                        if (data == null) buffers.AppendLine("  float4(0.0, 0.0, 0.0, 0.0), //null" + i);
                        else
                        {
                            try
                            {
                                if (data[i] is Vector4)
                                {
                                    buffers.AppendLine($"   float4({data[i].X}, {data[i].Y}, {data[i].Z}, {data[i].W}), //" + i);
                                }
                                else
                                {
                                    var x = data[i].Unk00.X; // really bad but required
                                    buffers.AppendLine($"   float4({x}, {data[i].Unk00.Y}, {data[i].Unk00.Z}, {data[i].Unk00.W}), //" + i);
                                }
                            }
                            catch (Exception e)  // figure out whats up here, taniks breaks it
                            {
                                buffers.AppendLine("    float4(0.0, 0.0, 0.0, 0.0), //Exception" + i);
                            }
                        }
                        break;
                    case "float3":
                        if (data == null) buffers.AppendLine("  float3(0.0, 0.0, 0.0), //null" + i);
                        else buffers.AppendLine($"  float3({data[i].Unk00.X}, {data[i].Unk00.Y}, {data[i].Unk00.Z}), //" + i);
                        break;
                    case "float":
                        if (data == null) buffers.AppendLine("  float(0.0), //null" + i);
                        else buffers.AppendLine($"  float4({data[i].Unk00}), //" + i);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            buffers.AppendLine("};");
        }

        if (buffers.ToString() != String.Empty)
        {
            try
            {
                File.WriteAllText($"{saveDirectory}/CBuffers/{(bIsVertexShader ? "CB_VS" : "CB_PS")}_{Hash}.txt", buffers.ToString());
                Console.WriteLine($"Saved CBuffers for material {Hash}");
            }
            catch (IOException)  // threading error
            {
            }
        }
    }
}

[StructLayout(LayoutKind.Sequential, Size = 0x3D0)]
public struct D2Class_AA6D8080
{
    public long FileSize;
    public uint Unk08;
    public uint Unk0C;
    public uint Unk10;

    [DestinyOffset(0x70), DestinyField(FieldType.TagHash)]
    public ShaderHeader VertexShader;
    [DestinyOffset(0x78), DestinyField(FieldType.TablePointer)]
    public List<D2Class_CF6D8080> VSTextures;
    [DestinyOffset(0x90), DestinyField(FieldType.TablePointer)]
    public List<D2Class_09008080> Unk90;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_90008080> UnkA0;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_3F018080> UnkB0;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_90008080> UnkC0;
    
    [DestinyOffset(0x2B0), DestinyField(FieldType.TagHash)]
    public ShaderHeader PixelShader;
    [DestinyOffset(0x2B8), DestinyField(FieldType.TablePointer)]
    public List<D2Class_CF6D8080> PSTextures;
    [DestinyOffset(0x2D0), DestinyField(FieldType.TablePointer)]
    public List<D2Class_09008080> Unk2D0;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_90008080> Unk2E0;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_3F018080> Unk2F0;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_90008080> Unk300;
    [DestinyOffset(0x324)] 
    public TagHash PSVector4Container;
    
    [DestinyOffset(0x340), DestinyField(FieldType.TagHash)]
    public ShaderHeader ComputeShader;
    [DestinyOffset(0x348), DestinyField(FieldType.TablePointer)]
    public List<D2Class_CF6D8080> CSTextures;
    [DestinyOffset(0x360), DestinyField(FieldType.TablePointer)]
    public List<D2Class_09008080> Unk360;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_90008080> Unk370;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_3F018080> Unk380;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_90008080> Unk390;
    
}

[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_CF6D8080
{
    public long TextureIndex;
    [DestinyField(FieldType.TagHash64)]
    public TextureHeader Texture;
}

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct D2Class_09008080
{
    public byte Value;
}

[StructLayout(LayoutKind.Sequential, Size = 0x10)]
public struct D2Class_3F018080
{
    [DestinyField(FieldType.TagHash64)]
    public Tag Unk00;
}
