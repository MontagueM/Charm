using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Field.General;
using Field.Models;

namespace Field;

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
    private bool bFixRoughness = false;
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
        bool success = ConvertInstructions();
        if (!success)
        {
            return "";
        }

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
                    texture.Index = Int32.TryParse(new string(texture.Variable.Skip(1).ToArray()), out int index) ? index : -1;
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
                    cbuffer.Index = Int32.TryParse(new string(cbuffer.Variable.Skip(2).ToArray()), out int index) ? index : -1;
                    cbuffer.Count = Int32.TryParse(new string(line.Split("[")[1].Split("]")[0]), out int count) ? count : -1;
                    cbuffer.Type = line.Split("cb")[0].Trim();
                    cbuffers.Add(cbuffer);
                }
                else if (line.Contains(" v") && line.Contains(" : ") && !line.Contains("?"))
                {
                    Input input = new Input();
                    input.Variable = "v" + line.Split("v")[1].Split(" : ")[0];
                    input.Index = Int32.TryParse(new string(input.Variable.Skip(1).ToArray()), out int index) ? index : -1;
                    input.Semantic = line.Split(" : ")[1].Split(",")[0];
                    input.Type = line.Split(" v")[0].Trim();
                    inputs.Add(input);
                }
                else if (line.Contains("out") && line.Contains(" : "))
                {
                    Output output = new Output();
                    output.Variable = "o" + line.Split(" o")[2].Split(" : ")[0];
                    output.Index = Int32.TryParse(new string(output.Variable.Skip(1).ToArray()), out int index) ? index : -1;
                    output.Semantic = line.Split(" : ")[1].Split(",")[0];
                    output.Type = line.Split("out ")[1].Split(" o")[0];
                    outputs.Add(output);
                }
            }

        } while (line != null);
    }

    private void WriteCbuffers(Material material, bool bIsVertexShader)
    {
        // Try to find matches, pixel shader has Unk2D0 Unk2E0 Unk2F0 Unk300 available
        foreach (var cbuffer in cbuffers)
        {
            if (bIsVertexShader)
                usf.AppendLine($"static {cbuffer.Type} {cbuffer.Variable}[{cbuffer.Count}] = ").AppendLine("{");
            else
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
                else
                {

                    // if (material.Header.VSVector4Container.Hash != 0xffff_ffff)
                    // {
                    //     // Try the Vector4 storage file
                    //     DestinyFile container = new DestinyFile(PackageHandler.GetEntryReference(material.Header.VSVector4Container));
                    //     byte[] containerData = container.GetData();
                    //     int num = containerData.Length / 16;
                    //     if (cbuffer.Count == num)
                    //     {
                    //         List<Vector4> float4s = new List<Vector4>();
                    //         for (int i = 0; i < containerData.Length / 16; i++)
                    //         {
                    //             float4s.Add(StructConverter.ToStructure<Vector4>(containerData.Skip(i*16).Take(16).ToArray()));
                    //         }

                    //         data = float4s;
                    //     }                        
                    // }
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
                        if (bIsVertexShader)
                        {
                            if (data == null)
                            {
                                usf.AppendLine("    float4(1.0, 1.0, 1.0, 1.0),");
                            }
                            break;
                        }

                        if (data == null)
                        {
                            usf.AppendLine("    float4(0.0, 0.0, 0.0, 0.0),");
                        }
                        else
                        {
                            try
                            {
                                if (data[i] is Vector4)
                                {
                                    usf.AppendLine($"    float4({data[i].X}, {data[i].Y}, {data[i].Z}, {data[i].W}),");
                                }
                                else
                                {
                                    var x = data[i].Unk00.X; // really bad but required
                                    usf.AppendLine($"    float4({x}, {data[i].Unk00.Y}, {data[i].Unk00.Z}, {data[i].Unk00.W}),");
                                }
                            }
                            catch (Exception e)  // figure out whats up here, taniks breaks it
                            {
                                if (bIsVertexShader)
                                {
                                    usf.AppendLine("    float4(1.0, 1.0, 1.0, 1.0),");
                                }
                                else
                                    usf.AppendLine("    float4(0.0, 0.0, 0.0, 0.0),");
                            }
                        }
                        break;
                    case "float3":
                        if (bIsVertexShader)
                        {
                            if (data == null)
                            {
                                usf.AppendLine("    float3(1.0, 1.0, 1.0),");
                            }
                            break;
                        }
                        if (data == null) usf.AppendLine("    float3(0.0, 0.0, 0.0),");
                        else usf.AppendLine($"    float3({data[i].Unk00.X}, {data[i].Unk00.Y}, {data[i].Unk00.Z}),");
                        break;
                    case "float":
                        if (bIsVertexShader)
                        {
                            if (data == null)
                            {
                                usf.AppendLine("    float(1.0),");
                            }
                            break;
                        }
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

            usf.AppendLine($"   float2 tx,");
            usf.AppendLine($"   float3 vc,");
            usf.AppendLine($"   float vcw)"); //UE5 Vertex color node doesnt support RGBA output for some reason?

            usf.AppendLine("{").AppendLine("    FMaterialAttributes output;");
            // Output render targets, todo support vertex shader
            usf.AppendLine("    float4 o0,o1,o2;");

            usf.AppendLine("        float4 v0 = {0,0,1,1};");
            usf.AppendLine("        float4 v1 = {1,0,0,1};");
            usf.AppendLine("        float4 v2 = {0,1,0,1};");
            usf.AppendLine("        float4 v3 = {tx.xy, 1,1};");
            usf.AppendLine("        float4 v4 = {1,1,1,1};");
            usf.AppendLine("        float4 v5 = {vc.xyz, vcw};");

            foreach (var i in inputs)
            {
                if (i.Type == "uint")
                {
                    usf.AppendLine($"    {i.Variable}.x = {i.Variable}.x;");
                }
            }
        }
    }

    private bool ConvertInstructions()
    {
        Dictionary<int, Texture> texDict = new Dictionary<int, Texture>();
        foreach (var texture in textures)
        {
            texDict.Add(texture.Index, texture);
        }
        List<int> sortedIndices = texDict.Keys.OrderBy(x => x).ToList();
        string line = hlsl.ReadLine();
        if (line == null)
        {
            // its a broken pixel shader that uses some kind of memory textures
            return false;
        }
        while (!line.Contains("SV_TARGET2"))
        {
            line = hlsl.ReadLine();
            if (line == null)
            {
                // its a broken pixel shader that uses some kind of memory textures
                return false;
            }
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
                    usf.AppendLine($"   {equal}= Material_Texture2D_{sortedIndices.IndexOf(texIndex)}.SampleLevel(Material_Texture2D_{sampleIndex - 1}Sampler, {sampleUv}, 0).{dotAfter}");
                }
                else if (line.Contains("CalculateLevelOfDetail"))
                {
                    var equal = line.Split("=")[0];
                    var texIndex = Int32.Parse(line.Split(".CalculateLevelOfDetail")[0].Split("t")[1]);
                    var sampleIndex = Int32.Parse(line.Split("(s")[1].Split("_s,")[0]);
                    var sampleUv = line.Split(", ")[1].Split(")")[0];

                    usf.AppendLine($"       {equal}= Material_Texture2D_{texIndex}.CalculateLevelOfDetail(Material_Texture2D_{sampleIndex - 1}Sampler, {sampleUv});");
                }
                // todo add load, levelofdetail, o0.w, discard
                else if (line.Contains("discard"))
                {
                    usf.AppendLine(line.Replace("discard", "{ output.OpacityMask = 0; return output; }"));
                }
                else if (line.Contains("o0.w = r")) //o0.w = r(?)
                {
                    usf.AppendLine($"       {line}");
                    usf.AppendLine("        { output.OpacityMask = 1 - o0.w; return output; }");
                }
                else if (line.Contains("Load"))
                {
                    var equal = line.Split("=")[0];
                    var texIndex = Int32.Parse(line.Split(".Load")[0].Split("t")[1]);
                    var sampleUv = line.Split("(")[1].Split(")")[0];

                    usf.AppendLine($"       {equal}= Material_Texture2D_{(int)texIndex + 1}.Load({sampleUv});"); //Usually seen in decals, the texture isnt actually valid though?
                }
                else if (line.Contains("o1.xyzw = float4(0,0,0,0);"))
                {
                    usf.AppendLine(line.Replace("o1.xyzw = float4(0,0,0,0);", "        o1.xyzw = float4(1,1,1,0);")); //decals(?) have 0 normals sometimes, dont want that
                    bFixRoughness = true;
                }
                else
                {
                    usf.AppendLine(line);
                }
            }
        } while (line != null);

        return true;
    }

    private void AddOutputs()
    {
        string outputString = @$"
        ///RT0
        output.BaseColor = o0.xyz; // Albedo
        
        ///RT1

        // Normal
        float3 biased_normal = o1.xyz - float3(0.5, 0.5, 0.5);
        float normal_length = length(biased_normal);
        float3 normal_in_world_space = biased_normal / normal_length;
        
        output.Normal = normal_in_world_space.xyz;

        // Roughness
        float smoothness = saturate(8 * ({(bFixRoughness ? "0" : "normal_length")} - 0.375)); 
        output.Roughness = 1 - smoothness;
 
        ///RT2
        output.Metallic = saturate(o2.x);
        output.EmissiveColor = clamp((o2.y - 0.5) * 2 * 5 * output.BaseColor, 0, 100);  // the *5 is a scale to make it look good
        output.AmbientOcclusion = saturate(o2.y * 2); // Texture AO

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
            usf.AppendLine("shader s;").AppendLine($"return s.main({String.Join(',', textures.Select(x => x.Variable))},tx,vc,vcw);");
        }
    }
}