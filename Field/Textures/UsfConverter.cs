using System.Text;
using Field.Models;

namespace Field.Textures;

public struct Texture
{
    public string Dimension;
    public string Type;
    public string Variable;
    public int Index;
}

public struct Cbuffer
{
    public string Variable;
    public string Type;
    public int Count;
    public int Index;
}

public struct Input
{
    public string Variable;
    public string Type;
    public int Index;
    public string Semantic;
}

public struct Output
{
    public string Variable;
    public string Type;
    public int Index;
    public string Semantic; 
}

public class UsfConverter
{
    private StringReader hlsl;
    private StringBuilder usf;
    private bool bOpacityEnabled = false;
    private List<Texture> textures = new List<Texture>();
    private List<int> samplers = new List<int>();
    private List<Cbuffer> cbuffers = new List<Cbuffer>();
    private List<Input> inputs = new List<Input>();
    private List<Output> outputs = new List<Output>();
    private int shaderInstructionStartIndex;
    
    public string HlslToUsf(Material material, string hlslText)
    {
        hlsl = new StringReader(hlslText);
        usf = new StringBuilder();
        bOpacityEnabled = false;
        ProcessHlslData();
        WriteTextureComments(material);
        WriteCbuffers(material);
        WriteFunctionDefinition();
        hlsl = new StringReader(hlslText);
        ConvertInstructions();
        if (bOpacityEnabled)
        {
            usf.Insert(1, "// masked");
        }

        AddOutputs();
        WriteFooter();
        return usf.ToString();
    }

    private void ProcessHlslData()
    {
        string line = string.Empty;
        do
        {
            line = hlsl.ReadLine();
            if (line != null)
            {
                if (line.Contains("bitmask")) // at end of function definition
                {
                    // todo maybe check to see if all the inputs are actually utilised
                    break;
                }

                shaderInstructionStartIndex++;
                if (line.Contains("Texture"))
                {
                    Texture texture = new Texture();
                    texture.Dimension = line.Split("<")[0];
                    texture.Type = line.Split("<")[1].Split(">")[0];
                    texture.Variable = line.Split("> ")[1].Split(" :")[0];
                    texture.Index = Int32.Parse(new string(texture.Variable.Skip(1).ToArray()));
                    textures.Add(texture);
                }
                else if (line.Contains("SamplerState"))
                {
                    samplers.Add(line.Split("(")[1].Split(")")[0].Last() - 48);
                }
                else if (line.Contains("cbuffer"))
                {
                    hlsl.ReadLine();
                    line = hlsl.ReadLine();
                    Cbuffer cbuffer = new Cbuffer();
                    cbuffer.Variable = "cb" + line.Split("cb")[1].Split("[")[0];
                    cbuffer.Index = Int32.Parse(new string(cbuffer.Variable.Skip(2).ToArray()));
                    cbuffer.Count = Int32.Parse(new string(line.Split("[")[1].Split("]")[0]));
                    cbuffer.Type = line.Split("cb")[0].Trim();
                    cbuffers.Add(cbuffer);
                }
                else if (line.Contains(" v") && line.Contains(" : "))
                {
                    Input input = new Input();
                    input.Variable = "v" + line.Split("v")[1].Split(" : ")[0];
                    input.Index = Int32.Parse(new string(input.Variable.Skip(1).ToArray()));
                    input.Semantic = line.Split(" : ")[1].Split(",")[0];
                    input.Type = line.Split(" v")[0].Trim();
                    inputs.Add(input);
                }
                else if (line.Contains("out") && line.Contains(" : "))
                {
                    Output output = new Output();
                    output.Variable = "o" + line.Split(" o")[2].Split(" : ")[0];
                    output.Index = Int32.Parse(new string(output.Variable.Skip(1).ToArray()));
                    output.Semantic = line.Split(" : ")[1].Split(",")[0];
                    output.Type = line.Split("out ")[1].Split(" o")[0];
                    outputs.Add(output);
                }
            }

        } while (line != null);
    }

    private void WriteTextureComments(Material material)
    {
        var textureIndices = textures.Select(x => x.Index);
        // todo vertex shader
        foreach (var inputTexture in material.Header.PSTextures.OrderBy(x => x.TextureIndex))
        {
            string hash = "NONE";
            if (textureIndices.Contains((int)inputTexture.TextureIndex))  // if our input texture indices match the textures in the shader
            {
                hash = inputTexture.Texture.Hash;
            }

            usf.AppendLine($"// t{inputTexture.TextureIndex} : {hash}");
        }
    }

    private void WriteCbuffers(Material material)
    {
        // Try to find matches, pixel shader has Unk2D0 Unk2E0 Unk2F0 Unk300 available
        foreach (var cbuffer in cbuffers)
        {
            usf.AppendLine($"static {cbuffer.Type} {cbuffer.Variable}[{cbuffer.Count}] = ").AppendLine("{");
            
            dynamic data = null; 
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

            for (int i = 0; i < cbuffer.Count; i++)
            {
                switch (cbuffer.Type)
                {
                    case "float4":
                        if (data == null) usf.AppendLine("    float4(0.0, 0.0, 0.0, 0.0),");
                        else usf.AppendLine($"    float4({data[i].Unk00.X}, {data[i].Unk00.Y}, {data[i].Unk00.Z}, {data[i].Unk00.W}),");
                        break;
                    case "float3":
                        if (data == null) usf.AppendLine("    float3(0.0, 0.0, 0.0),");
                        else usf.AppendLine($"    float3({data[i].Unk00.X}, {data[i].Unk00.Y}, {data[i].Unk00.Z}),");
                        break;
                    case "float":
                        if (data == null) usf.AppendLine("    float(0.0),");
                        else usf.AppendLine($"    float4({data[i].Unk00}),");
                        break;
                    default:
                        throw new NotImplementedException();
                }  
            }

            usf.AppendLine("};");
        }
    }
    
    private void WriteFunctionDefinition()
    {
        usf.AppendLine("#define cmp -").AppendLine("struct shader {").AppendLine("FMaterialAttributes main(");
        foreach (var texture in textures)
        {
            usf.AppendLine($"   {texture.Type} {texture.Variable},");
        }
        for (var i = 0; i < inputs.Count; i++)
        {
            if (i == inputs.Count - 1)
            {
                usf.AppendLine($"   {inputs[i].Type} {inputs[i].Variable})");
            }
            else
            {
                usf.AppendLine($"   {inputs[i].Type} {inputs[i].Variable},");
            }
        }

        usf.AppendLine("{").AppendLine("    FMaterialAttributes output;");
        // Output render targets, todo support vertex shader
        usf.AppendLine("    float4 o0,o1,o2;");
    }

    private void ConvertInstructions()
    {
        Dictionary<int, Texture> texDict = new Dictionary<int, Texture>();
        foreach (var texture in textures)
        {
            texDict.Add(texture.Index, texture);
        }
        
        for (int i = 0; i < shaderInstructionStartIndex+1; i++) hlsl.ReadLine();
        
        string line = string.Empty;
        do
        {
            line = hlsl.ReadLine();
            if (line != null)
            {
                if (line.Contains("return;")) break;

                if (line.Contains("Sample"))
                {
                    var equal = line.Split("=")[0];
                    var texIndex = Int32.Parse(line.Split(".Sample")[0].Split("t")[1]);
                    var sampleIndex = Int32.Parse(line.Split("(s")[1].Split("_s,")[0]);
                    var sampleUv = line.Split(", ")[1].Split(")")[0];
                    var dotAfter = line.Split(").")[1];
                    // todo add dimension
                    usf.AppendLine($"   {equal}= Material_Texture2D_{texDict[texIndex].Index}.SampleLevel(Material_Texture2D_{sampleIndex-1}Sampler, {sampleUv}, 0).{dotAfter}");
                }
                // todo add load, levelofdetail, o0.w, discard
                else if (line.Contains("discard"))
                {
                    usf.AppendLine(line.Replace("discard", "{ output.OpacityMask = 0; return output; }"));
                }
                else
                {
                    usf.AppendLine(line);
                }
            }
        } while (line != null);
    }

    private void AddOutputs()
    {
        string outputString = @"
        ///RT0
        output.BaseColor = o0.xyz; // Albedo
        
        ///RT1

        // Normal
        float3 biased_normal = o1.xyz - float3(0.5, 0.5, 0.5);
        float normal_length = length(biased_normal);
        float3 normal_in_world_space = biased_normal / normal_length;
        output.Normal = float3(normal_in_world_space.x, normal_in_world_space.y, normal_in_world_space.z);

        // Roughness
        float smoothness = saturate(8 * (normal_length - 0.375));
        output.Roughness = 1 - smoothness;
 
        ///RT2
        output.Metallic = o2.x;
        output.EmissiveColor = (o2.y - 0.5) * 2;
        output.AmbientOcclusion = o2.y * 2; // Texture AO

        return output;
        ";
        usf.AppendLine(outputString);
    }

    private void WriteFooter()
    {
        usf.AppendLine("}").AppendLine("};").AppendLine("shader s;").AppendLine($"return s.main({String.Join(',', textures.Select(x => x.Variable))},{String.Join(',', inputs.Select(x => x.Variable))});");
    }
}