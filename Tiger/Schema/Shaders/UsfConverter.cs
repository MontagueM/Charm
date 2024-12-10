using System.Text;
using Tiger.Schema.Shaders;

namespace Tiger.Schema;

public class UsfConverter
{
    private struct TextureView
    {
        public string Dimension;
        public string Type;
        public string Variable;
        public int Index;
    }

    private struct Cbuffer
    {
        public string Variable;
        public string Type;
        public int Count;
        public int Index;
    }

    private struct Input
    {
        public string Variable;
        public string Type;
        public int Index;
        public string Semantic;
    }

    private struct Output
    {
        public string Variable;
        public string Type;
        public int Index;
        public string Semantic;
    }

    private StringReader hlsl;
    private StringBuilder usf;
    private bool bOpacityEnabled = false;
    private readonly List<TextureView> textures = new List<TextureView>();
    private readonly List<int> samplers = new List<int>();
    private readonly List<Cbuffer> cbuffers = new List<Cbuffer>();
    private readonly List<Input> inputs = new List<Input>();
    private readonly List<Output> outputs = new List<Output>();

    public string HlslToUsf(Material material, bool bIsVertexShader)
    {
        hlsl = new StringReader(material.Pixel.Shader.Decompile($"ps{material.Pixel.Shader.Hash}"));
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
                    TextureView texture = new TextureView();
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
                data = material.Vertex.GetCBuffer0();
            }
            else
            {
                data = material.Pixel.GetCBuffer0();
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
                usf.Replace("v0.xyzw = v0.xyzw * tx.xyxy;", "v0.xyzw = v0.xyzw;");
            }
        }
    }

    private bool ConvertInstructions()
    {
        Dictionary<int, TextureView> texDict = new();
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

        return true;
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
        normal_in_world_space.z = sqrt(1.0 - saturate(dot(normal_in_world_space.xy, normal_in_world_space.xy)));
        output.Normal = normalize((normal_in_world_space * 2 - 1.35)*0.5 + 0.5);

        // Roughness
        float smoothness = saturate(8 * (normal_length - 0.375));
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
            usf.AppendLine("shader s;").AppendLine($"return s.main({String.Join(',', textures.Select(x => x.Variable))},tx);");
        }
    }
}
