using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Field.General;
using File = System.IO.File;

namespace Field.Textures;

public struct ExportSettings { public bool Raw, Unreal, Blender, Source2; }

public class Material : Tag {
    
    public D2Class_AA6D8080 Header;
    
    private static readonly object Lock = new();

    public Material(TagHash hash) : base(hash) { }

    protected override void ParseStructs() {
        Header = ReadHeader<D2Class_AA6D8080>();
    }

    public void Export(string savePath, ExportSettings settings) {
        var texturePath = $"{savePath}/Textures";
        Directory.CreateDirectory(texturePath);
        var matPath = $"{savePath}/Materials";
        Directory.CreateDirectory(matPath);
        SaveAllTextures(texturePath, Header.PSTextures, Header.VSTextures, Header.CSTextures);
        if (!File.Exists($"{matPath}/{Hash}_meta.json")) {
            try { File.WriteAllText($"{matPath}/{Hash}_meta.json", CreateTextureManifest().ToJsonString(new JsonSerializerOptions { WriteIndented = true })); }
            catch(IOException ignored) { }
        }
        if(settings.Raw) {
            var rawPath = $"{matPath}/Raw";
            Directory.CreateDirectory(rawPath);
            ExportMaterialRaw(rawPath);
            Console.WriteLine($"Successfully exported raw Material {Hash}.");
        }
        if(settings.Blender) {
            var blenderPath = $"{matPath}/Blender";
            Directory.CreateDirectory(blenderPath);
            ExportMaterialBlender(blenderPath);
            Console.WriteLine($"Successfully exported Blender Material {Hash}.");
        }
        if(settings.Unreal) {
            var unrealPath = $"{matPath}/Unreal";
            Directory.CreateDirectory(unrealPath);
            ExportMaterialUnreal(unrealPath);
            Console.WriteLine($"Successfully exported Unreal Material {Hash}.");
        }
        if(settings.Source2) {
            var s2Path = $"{matPath}/Source2";
            Directory.CreateDirectory(s2Path);
            ExportMaterialSource2(s2Path);
            Console.WriteLine($"Successfully exported Source2 Material {Hash}.");
        }
    }
    
    private string Decompile(ShaderType type, bool allowretry = true) {
        var directory = "hlsl_temp";
        Directory.CreateDirectory(directory);
        directory = Path.GetFullPath(directory);
        var fileName = $"{directory}\\{GetShaderPrefix(type)}_{Hash}";
        var binFile = $"{fileName}.bin";
        var hlslFile = $"{fileName}.hlsl";
        
        var bytecode = type switch {
            ShaderType.Pixel => Header.PixelShader.GetBytecode(),
            ShaderType.Vertex => Header.VertexShader.GetBytecode(),
            _ => Header.ComputeShader.GetBytecode()
        };
        
        lock (Lock) {
            if (!File.Exists(binFile))
                File.WriteAllBytes(binFile, bytecode);
        }

        if (!File.Exists(hlslFile)) {
            var startInfo = new ProcessStartInfo {
                CreateNoWindow = false,
                UseShellExecute = false,
                FileName = "3dmigoto_shader_decomp.exe",
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = $"-D {binFile}"
            };
            
            using (var exeProcess = Process.Start(startInfo)) {
                exeProcess?.WaitForExit();
            }

            if(!File.Exists(hlslFile)) {
                if(allowretry && File.Exists(binFile)) {
                    File.Delete(binFile);
                    return Decompile(type, false);
                }
                throw new FileNotFoundException($"Decompilation failed for {Hash}");
            }
        }

        var hlsl = "";
        lock (Lock) {
            while (hlsl == "") {
                try { hlsl = File.ReadAllText(hlslFile); }
                catch (IOException) { Thread.Sleep(100); }
            }
        }
        
        return hlsl;
    }
    
    private static void SaveAllTextures(string saveDirectory, params List<D2Class_CF6D8080>[] shaderTextures) {
        foreach(var textures in shaderTextures) {
            foreach (var e in textures.Where(e => e.Texture != null)) {
                var path = $"{saveDirectory}/{e.Texture.Hash}";
                if (!File.Exists(path + GetTextureExtension(TextureExtractor.Format)))
                    e.Texture.SavetoFile(path);
            }
        }
    }

    #region Texture Manifest

    private JsonObject CreateTextureManifest() {
        var root = new JsonObject();
        var textureMeta = new JsonObject();
        if(Header.PixelShader != null) {
            var i = CreateShaderIndices(Header.PSTextures);
            if(i.Count > 0)
                root["pixelShader"] = new JsonObject { ["indices"] = i };
            GetShaderTextureMeta(textureMeta, Header.PSTextures);
        }
        if(Header.VertexShader != null) {
            var i = CreateShaderIndices(Header.VSTextures);
            if(i.Count > 0)
                root["vertexShader"] = new JsonObject { ["indices"] = i };
            GetShaderTextureMeta(textureMeta, Header.VSTextures);
        }
        if(Header.ComputeShader != null) {
            var i = CreateShaderIndices(Header.CSTextures);
            if(i.Count > 0)
                root["computeShader"] = new JsonObject { ["indices"] = i };
            GetShaderTextureMeta(textureMeta, Header.CSTextures);
        }
        root["textures"] = textureMeta;
        root["format"] = GetTextureExtension(TextureExtractor.Format);
        return root;
    }
    
    private static void GetShaderTextureMeta(JsonObject table, List<D2Class_CF6D8080> textures) {
        foreach(var e in textures) {
            if(table.ContainsKey(e.Texture.Hash))
                continue;
            var meta = new JsonObject {
                ["srgb"] = e.Texture.IsSrgb(),
                ["volume"] = e.Texture.IsVolume(),
                ["cubemap"] = e.Texture.IsCubemap()
            };
            table[e.Texture.Hash] = meta;
        }
    }
    
    private static JsonObject CreateShaderIndices(List<D2Class_CF6D8080> textures) {
        var textureMeta = new JsonObject();
        foreach(var e in textures)
            textureMeta[e.TextureIndex.ToString()] = e.Texture.Hash.ToString();
        return textureMeta;
    }
    
    #endregion
    
    #region Platform-specific exports

    private void ExportMaterialRaw(string path) {
        if(Header.PixelShader != null) {
            try { File.WriteAllText($"{path}/{GetShaderPrefix(ShaderType.Pixel)}_{Hash}.hlsl", Decompile(ShaderType.Pixel)); }
            catch (IOException ignored) { }
            Console.WriteLine($"Exported raw PixelShader for Material {Hash}.");
        }
        if(Header.VertexShader != null) {
            try { File.WriteAllText($"{path}/{GetShaderPrefix(ShaderType.Vertex)}_{Hash}.hlsl", Decompile(ShaderType.Vertex)); }
            catch (IOException ignored) { }
            Console.WriteLine($"Exported raw VertexShader for Material {Hash}.");
        }
        if(Header.ComputeShader != null) {
            try { File.WriteAllText($"{path}/{GetShaderPrefix(ShaderType.Compute)}_{Hash}.hlsl", Decompile(ShaderType.Compute)); }
            catch (IOException ignored) { }
            Console.WriteLine($"Exported raw ComputeShader for Material {Hash}.");
        }
    }

    private void ExportMaterialBlender(string path) {
        // TODO: Merge Vertex and Pixel Shaders if applicable
        // TODO: Create a proper import script, assuming textures are bound and loaded
        if(Header.PixelShader != null) {
            var bpy = new NodeConverter().HlslToBpy(this, $"{path}/../..", Decompile(ShaderType.Pixel), false);
            if(bpy != string.Empty) {
                try { File.WriteAllText($"{path}/{GetShaderPrefix(ShaderType.Pixel)}_{Hash}.py", bpy); }
                catch (IOException ignored) { }
                Console.WriteLine($"Exported Blender PixelShader {Hash}.");
            }
        }
        if(Header.VertexShader != null) {
            var bpy = new NodeConverter().HlslToBpy(this, $"{path}/../..", Decompile(ShaderType.Vertex), true);
            if(bpy != string.Empty) {
                try { File.WriteAllText($"{path}/{GetShaderPrefix(ShaderType.Vertex)}_{Hash}.py", bpy); }
                catch (IOException ignored) { }
                Console.WriteLine($"Exported Blender VertexShader {Hash}.");
            }
        }
        // I don't think Blender can even handle compute shaders, so I'll leave that out. If it can, it's the same as above.
    }

    private void ExportMaterialUnreal(string path) {
        if(Header.PixelShader != null) {
            var usf = new UsfConverter().HlslToUsf(this, Decompile(ShaderType.Pixel), false);
            if(usf != string.Empty) {
                try { File.WriteAllText($"{path}/{GetShaderPrefix(ShaderType.Pixel)}_{Hash}.usf", usf); }
                catch (IOException ignored) { }
                Console.WriteLine($"Exported Unreal PixelShader {Hash}.");
            }
        }
        if(Header.VertexShader != null) {
            var usf = new UsfConverter().HlslToUsf(this, Decompile(ShaderType.Vertex), true);
            if(usf != string.Empty) {
                try { File.WriteAllText($"{path}/{GetShaderPrefix(ShaderType.Vertex)}_{Hash}.usf", usf); }
                catch (IOException ignored) { }
                Console.WriteLine($"Exported Unreal VertexShader {Hash}.");
            }
        }
    }

    private void ExportMaterialSource2(string path) {
        if(Header.PixelShader != null) {
            var vfx = new VfxConverter().HlslToVfx(this, Decompile(ShaderType.Pixel), false);
            if(vfx != string.Empty) {
                try { File.WriteAllText($"{path}/{GetShaderPrefix(ShaderType.Pixel)}_{Hash}.vfx", vfx); }
                catch (IOException ignored) { }
                Console.WriteLine($"Exported Source 2 PixelShader {Hash}.");
            }
            var materialBuilder = new StringBuilder("Layer0 \n{");
            materialBuilder.AppendLine($"\n\tshader \"{GetShaderPrefix(ShaderType.Pixel)}_{Hash}.vfx\"");
            materialBuilder.AppendLine("\tF_ALPHA_TEST 1");
            foreach(var e in Header.PSTextures.Where(e => e.Texture != null))
                materialBuilder.AppendLine($"\tTextureT{e.TextureIndex} \"materials/Textures/{e.Texture.Hash}{GetTextureExtension(TextureExtractor.Format)}\"");
            materialBuilder.AppendLine("}");
            Directory.CreateDirectory($"{path}/materials");
            try { File.WriteAllText($"{path}/materials/{Hash}.vmat", materialBuilder.ToString()); }
            catch (IOException ignored) { }
        }
    }
    
    #endregion

    #region Utils
    
    private static string GetTextureExtension(ETextureFormat format) {
        return format switch {
            ETextureFormat.PNG => ".png",
            ETextureFormat.TGA => ".tga",
            _ => ".dds"
        };
    }

    private static string GetShaderPrefix(ShaderType type) {
        return type switch {
            ShaderType.Pixel => "PS",
            ShaderType.Vertex => "VS",
            _ => "PS"
        };
    }

    private enum ShaderType { Pixel, Vertex, Compute }
    
    #endregion
}

#region Symmetry

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

#endregion
