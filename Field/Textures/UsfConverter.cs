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
    
    public string HlslToUsf(Material material, string hlslText, bool bIsVertexShader)
    {
        hlsl = new StringReader(hlslText);
        usf = new StringBuilder();
        bOpacityEnabled = false;
        ProcessHlslData();
        if (bOpacityEnabled)
        {
            usf.AppendLine("// masked");
        }
        // WriteTextureComments(material, bIsVertexShader);
        WriteCbuffers(material, bIsVertexShader);
        WriteFunctionDefinition(bIsVertexShader);
        hlsl = new StringReader(hlslText);
        ConvertInstructions();

        if (!bIsVertexShader)
        {
            AddOutputs();
        }

        WriteFooter(bIsVertexShader);
        return usf.ToString();
    }

    private void ProcessHlslData()
    {
        string line = string.Empty;
        bool bFindOpacity = false;
        do
        {
            line = hlsl.ReadLine();
            if (line != null)
            {
                if (line.Contains("r0,r1")) // at end of function definition
                {
                    bFindOpacity = true;
                }

                if (bFindOpacity)
                {
                    if (line.Contains("discard"))
                    {
                        bOpacityEnabled = true;
                        break;
                    }
                    continue;
                }

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

    private void WriteTextureComments(Material material, bool bIsVertexShader)
    {
        var textureIndices = textures.Select(x => x.Index);
        var array = material.Header.PSTextures.OrderBy(x => x.TextureIndex);
        if (bIsVertexShader) array = material.Header.VSTextures.OrderBy(x => x.TextureIndex);
        foreach (var inputTexture in array)
        {
            string hash = "NONE";
            if (textureIndices.Contains((int)inputTexture.TextureIndex))  // if our input texture indices match the textures in the shader
            {
                hash = inputTexture.Texture.Hash;
            }

            usf.AppendLine($"// t{inputTexture.TextureIndex} : {hash}");
        }
    }

    private void WriteCbuffers(Material material, bool bIsVertexShader)
    {
        // Try to find matches, pixel shader has Unk2D0 Unk2E0 Unk2F0 Unk300 available
        foreach (var cbuffer in cbuffers)
        {
            usf.AppendLine($"static {cbuffer.Type} {cbuffer.Variable}[{cbuffer.Count}] = ").AppendLine("{");
            
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
    
    private void WriteFunctionDefinition(bool bIsVertexShader)
    {
        if (!bIsVertexShader)
        {
            foreach (var i in inputs)
            {
                if (i.Type == "float4")
                {
                    usf.AppendLine($"static {i.Type} {i.Variable} = " + "{1, 1, 1, 1};\n");
                }
                else if (i.Type == "float3")
                {
                    usf.AppendLine($"static {i.Type} {i.Variable} = " + "{1, 1, 1};\n");
                }
                else if (i.Type == "uint")
                {
                    usf.AppendLine($"static {i.Type} {i.Variable} = " + "1;\n");
                }
            }
        }
        usf.AppendLine("#define cmp -").AppendLine("struct shader {");
        if (bIsVertexShader)
        {
            foreach (var output in outputs)
            {
                usf.AppendLine($"{output.Type} {output.Variable};");
            }

            usf.AppendLine().AppendLine("void main(");
            foreach (var texture in textures)
            {
                usf.AppendLine($"   {texture.Type} {texture.Variable},");
            }
            for (var i = 0; i < inputs.Count; i++)
            {
                if (i == inputs.Count - 1)
                {
                    usf.AppendLine($"   {inputs[i].Type} {inputs[i].Variable}) // {inputs[i].Semantic}");
                }
                else
                {
                    usf.AppendLine($"   {inputs[i].Type} {inputs[i].Variable}, // {inputs[i].Semantic}");
                }
            }
        }
        else
        {
            usf.AppendLine("FMaterialAttributes main(");
            foreach (var texture in textures)
            {
                usf.AppendLine($"   {texture.Type} {texture.Variable},");
            }

            usf.AppendLine($"   float2 tx)");

            usf.AppendLine("{").AppendLine("    FMaterialAttributes output;");
            // Output render targets, todo support vertex shader
            usf.AppendLine("    float4 o0,o1,o2;");
            foreach (var i in inputs)
            {
                if (i.Type == "float4")
                {
                    usf.AppendLine($"    {i.Variable}.xyzw = {i.Variable}.xyzw * tx.xyxy;");
                }
                else if (i.Type == "float3")
                {
                    usf.AppendLine($"    {i.Variable}.xyz = {i.Variable}.xyz * tx.xyx;");
                }
                else if (i.Type == "uint")
                {
                    usf.AppendLine($"    {i.Variable}.x = {i.Variable}.x * tx.x;");
                }
            }
        }
    }

    private void ConvertInstructions()
    {
        Dictionary<int, Texture> texDict = new Dictionary<int, Texture>();
        foreach (var texture in textures)
        {
            texDict.Add(texture.Index, texture);
        }
        string line = hlsl.ReadLine();

        while (!line.Contains("SV_TARGET2"))
        {
            line = hlsl.ReadLine();
        }
        hlsl.ReadLine();
        do
        {
            line = hlsl.ReadLine();
            if (line != null)
            {
                if (line.Contains("return;"))
                {
                    break;
                }
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
        output.Normal = float3(normal_in_world_space.x, normal_in_world_space.z, normal_in_world_space.y);
		//output.Normal = Material_Texture2D_2.SampleLevel(Material_Texture2D_0Sampler, v3.xy, 0).xyz;
        //output.Normal.z = sqrt(1.0 - saturate(dot(output.Normal.xy, output.Normal.xy)));
        //output.Normal = normalize(output.Normal);

        // Roughness
        float smoothness = saturate(8 * (normal_length - 0.375));
        output.Roughness = 1 - smoothness;
 
        ///RT2
        output.Metallic = o2.x;
        output.EmissiveColor = (o2.y - 0.5) * 2;
        output.AmbientOcclusion = o2.y * 2; // Texture AO

        output.OpacityMask = 1;

        return output;
        ";
        usf.AppendLine(outputString);
    }

    private void WriteFooter(bool bIsVertexShader)
    {
        usf.AppendLine("}").AppendLine("};");
        if (!bIsVertexShader)
        {
            usf.AppendLine("shader s;").AppendLine($"return s.main({String.Join(',', textures.Select(x => x.Variable))},tx);");
        }
    }
}