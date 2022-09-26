using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Field.General;
using Field.Models;

namespace Field.Textures;


public class NodeConverter {
    
    private string raw_hlsl, Hash;
    private StringReader hlsl;
    private StringBuilder bpy;
    private bool bOpacityEnabled = false;
    private List<Texture> textures = new();
    private List<int> samplers = new();
    private List<Cbuffer> cbuffers = new ();
    private List<Input> inputs = new();
    private List<Output> outputs = new();
    
    public string HlslToBpy(Material material, string saveDir, string hlslText, bool bIsVertexShader) {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        this.raw_hlsl = hlslText;
        this.Hash = material.Hash;
        hlsl = new StringReader(hlslText);
        bpy = new StringBuilder();
        bOpacityEnabled = false;
        ProcessHlslData();
        if (bOpacityEnabled)
            bpy.AppendLine("# masked");
        // WriteTextureComments(material, bIsVertexShader);
        WriteCbuffers(material, bIsVertexShader);
        //WriteFunctionDefinition(bIsVertexShader);
        hlsl = new StringReader(hlslText);
        bool success = ConvertInstructions();
        if (!success)
            return "";

        if (!bIsVertexShader) { /*AddOutputs();*/ }
        //WriteFooter(bIsVertexShader);
        
        var template = File.ReadAllText("import_mat_to_blender.py");
        template = template.Replace("<<<MAT_NAME>>>", material.Hash);
        template = template.Replace("<<<SHADER_TYPE>>>", bIsVertexShader ? "vertexShader" : "pixelShader");
        template = template.Replace("<<<EXPORT_PATH>>>", saveDir.Replace("\\", "\\\\"));
        return template.Replace("# <<<REPLACE WITH SCRIPT>>>", "\t" + bpy.ToString().Replace("\n", "\n        "));
    }

    private void ProcessHlslData() {
        var line = string.Empty;
        var bFindOpacity = false;
        do {
            line = hlsl.ReadLine();
            if (line != null) {
                if (line.Contains("r0,r1")) // at end of function definition
                    bFindOpacity = true;

                if (bFindOpacity) {
                    if (line.Contains("discard")) {
                        bOpacityEnabled = true;
                        break;
                    }
                    continue;
                }

                if (line.Contains("Texture")) {
                    var texture = new Texture();
                    texture.Dimension = line.Split("<")[0];
                    texture.Type = line.Split("<")[1].Split(">")[0];
                    texture.Variable = line.Split("> ")[1].Split(" :")[0];
                    texture.Index = Int32.TryParse(new string(texture.Variable.Skip(1).ToArray()), out int index) ? index : -1;
                    textures.Add(texture);
                } else if (line.Contains("SamplerState")) {
                    samplers.Add(line.Split("(")[1].Split(")")[0].Last() - 48);
                } else if (line.Contains("cbuffer")) {
                    hlsl.ReadLine();
                    line = hlsl.ReadLine();
                    var cbuffer = new Cbuffer { Variable = "cb" + line.Split("cb")[1].Split("[")[0] };
                    cbuffer.Index = Int32.TryParse(new string(cbuffer.Variable.Skip(2).ToArray()), out int index) ? index : -1;
                    cbuffer.Count = Int32.TryParse(new string(line.Split("[")[1].Split("]")[0]), out int count) ? count : -1;
                    cbuffer.Type = line.Split("cb")[0].Trim();
                    cbuffers.Add(cbuffer);
                } else if (line.Contains(" v") && line.Contains(" : ") && !line.Contains("?")) {
                    var input = new Input { Variable = "v" + line.Split("v")[1].Split(" : ")[0] };
                    input.Index = Int32.TryParse(new string(input.Variable.Skip(1).ToArray()), out int index) ? index : -1;
                    input.Semantic = line.Split(" : ")[1].Split(",")[0];
                    input.Type = line.Split(" v")[0].Trim();
                    inputs.Add(input);
                } else if (line.Contains("out") && line.Contains(" : ")) {
                    var output = new Output();
                    output.Variable = "o" + line.Split(" o")[2].Split(" : ")[0];
                    output.Index = Int32.TryParse(new string(output.Variable.Skip(1).ToArray()), out int index) ? index : -1;
                    output.Semantic = line.Split(" : ")[1].Split(",")[0];
                    output.Type = line.Split("out ")[1].Split(" o")[0];
                    outputs.Add(output);
                }
            }
        } while (line != null);
    }

    private void WriteCbuffers(Material material, bool bIsVertexShader) {
        bpy.AppendLine("### CBUFFERS ###");
        // Try to find matches, pixel shader has Unk2D0 Unk2E0 Unk2F0 Unk300 available
        foreach (var cbuffer in cbuffers) {
            bpy.AppendLine($"#static {cbuffer.Type} {cbuffer.Variable}[{cbuffer.Count}]").AppendLine();
            
            dynamic data = null;
            if (bIsVertexShader) {
                if (cbuffer.Count == material.Header.Unk90.Count)
                    data = material.Header.Unk90;
                else if (cbuffer.Count == material.Header.UnkA0.Count)
                    data = material.Header.UnkA0;
                else if (cbuffer.Count == material.Header.UnkB0.Count)
                    data = material.Header.UnkB0;
                else if (cbuffer.Count == material.Header.UnkC0.Count)
                    data = material.Header.UnkC0;
            } else {
                if (cbuffer.Count == material.Header.Unk2D0.Count)
                    data = material.Header.Unk2D0;
                else if (cbuffer.Count == material.Header.Unk2E0.Count)
                    data = material.Header.Unk2E0;
                else if (cbuffer.Count == material.Header.Unk2F0.Count)
                    data = material.Header.Unk2F0;
                else if (cbuffer.Count == material.Header.Unk300.Count)
                    data = material.Header.Unk300;
                else {
                    if (material.Header.PSVector4Container.Hash != 0xffff_ffff) {
                        // Try the Vector4 storage file
                        var container = new DestinyFile(PackageHandler.GetEntryReference(material.Header.PSVector4Container));
                        var containerData = container.GetData();
                        var num = containerData.Length / 16;
                        if (cbuffer.Count == num) {
                            var float4S = new List<Vector4>();
                            for (var i = 0; i < containerData.Length / 16; i++)
                                float4S.Add(containerData.Skip(i*16).Take(16).ToArray().ToStructure<Vector4>());
                            data = float4S;
                        }                        
                    }
                }
            }


            for (var i = 0; i < cbuffer.Count; i++) {
                switch (cbuffer.Type) {
                    case "float4":
                        if (data == null) 
                            bpy.AppendLine($"add_float4('{cbuffer.Variable}[{i}]', 0.0, 0.0, 0.0, 0.0)");
                        else {
                            try {
                                if (data[i] is Vector4)
                                    bpy.AppendLine($"add_float4('{cbuffer.Variable}[{i}]', {data[i].X}, {data[i].Y}, {data[i].Z}, {data[i].W})");
                                else {
                                    var x = data[i].Unk00.X; // really bad but required
                                    bpy.AppendLine($"add_float4('{cbuffer.Variable}[{i}]', {x}, {data[i].Unk00.Y}, {data[i].Unk00.Z}, {data[i].Unk00.W})");
                                }
                            } catch (Exception e) { // figure out whats up here, taniks breaks it
                                bpy.AppendLine($"add_float4('{cbuffer.Variable}[{i}]', 0.0, 0.0, 0.0, 0.0)");
                            }
                        }
                        break;
                    case "float3":
                        if (data == null)
                            bpy.AppendLine($"add_float3('{cbuffer.Variable}[{i}]', 0.0, 0.0, 0.0),");
                        else 
                            bpy.AppendLine($"add_float3('{cbuffer.Variable}[{i}]', {data[i].Unk00.X}, {data[i].Unk00.Y}, {data[i].Unk00.Z}),");
                        break;
                    case "float":
                        if (data == null)
                            bpy.AppendLine($"add_float('{cbuffer.Variable}[{i}]', 0.0)");
                        else
                            bpy.AppendLine($"add_float4('{cbuffer.Variable}[{i}]', {data[i].Unk00}, 0.0, 0.0, 0.0)");
                        break;
                    default:
                        throw new NotImplementedException();
                }  
            }
            bpy.AppendLine("");
        }
    }
    
    private void WriteFunctionDefinition(bool bIsVertexShader) {
        bpy.AppendLine("### Function Definition ###");
        if (!bIsVertexShader) {
            bpy.AppendLine("### v[n] vars ###");
            //foreach (var i in inputs)
            //{
                //if (i.Variable == "v0")
                //{
                //    bpy.AppendLine($"addFloat4('v0', 1.0, 1.0, 1.0, 1.0)\n");
                //}
                //else if (i.Type == "float4")
                //{                    
                //    bpy.AppendLine($"variable_dict['{i.Variable}.x'] = variable_dict['tx.x']");
                //    bpy.AppendLine($"variable_dict['{i.Variable}.y'] = variable_dict['tx.y']");
                //    bpy.AppendLine($"variable_dict['{i.Variable}.z'] = variable_dict['tx.x']");
                //    bpy.AppendLine($"variable_dict['{i.Variable}.w'] = variable_dict['tx.y']");
                //}
                //else if (i.Type == "float3")
                //{
                //    bpy.AppendLine($"variable_dict['{i.Variable}.x'] = variable_dict['tx.x']");
                //    bpy.AppendLine($"variable_dict['{i.Variable}.y'] = variable_dict['tx.y']");
                //    bpy.AppendLine($"variable_dict['{i.Variable}.z'] = variable_dict['tx.x']");
                //}
                //else if (i.Type == "uint")
                //{
                //    bpy.AppendLine($"variable_dict['{i.Variable}.x'] = variable_dict['tx.x']");
                //}
            //}
        }
        //bpy.AppendLine("#define cmp -").AppendLine("struct shader {");

        //Variable Definitions not needed
        if (bIsVertexShader) {
            bpy.AppendLine("### Is Vertex Shader ###");
            //foreach (var output in outputs)
            //{
            //    bpy.AppendLine($"{output.Type} {output.Variable};");
            //}

            //bpy.AppendLine().AppendLine("void main(");
            //foreach (var texture in textures)
            //{
            //    bpy.AppendLine($"   {texture.Type} {texture.Variable},");
            //}
            //for (var i = 0; i < inputs.Count; i++)
            //{
            //    if (i == inputs.Count - 1)
            //    {
            //        bpy.AppendLine($"   {inputs[i].Type} {inputs[i].Variable}) // {inputs[i].Semantic}");
            //    }
            //    else
            //    {
            //        bpy.AppendLine($"   {inputs[i].Type} {inputs[i].Variable}, // {inputs[i].Semantic}");
            //    }
            //}
        } else {
            bpy.AppendLine("### Is Not Vertex Shader ###");
            //bpy.AppendLine("FMaterialAttributes main(");
            //foreach (var texture in textures)
            //{
            //    bpy.AppendLine($"   {texture.Type} {texture.Variable},");
            //}

            //bpy.AppendLine($"   float2 tx)");

            //bpy.AppendLine("{").AppendLine("    FMaterialAttributes output;");
            // Output render targets, todo support vertex shader
            //bpy.AppendLine("    float4 o0,o1,o2;");
            //foreach (var i in inputs)
            //{
            //    //Not sure if this is needed for blender tbh, but add it to the top of the hlsl just to be safe
            //    if (i.Type == "float4")
            //    {
            //        raw_hlsl = $"{i.Variable}.xyzw = {i.Variable}.xyzw * tx.xyxy;" + raw_hlsl;
            //    }
            //    else if (i.Type == "float3")
            //    {
            //        raw_hlsl = $"{i.Variable}.xyz = {i.Variable}.xyz * tx.xyx;" + raw_hlsl;
            //    }
            //    else if (i.Type == "uint")
            //    {
            //        raw_hlsl = $"{i.Variable}.x = {i.Variable}.x * tx.x;" + raw_hlsl;
            //    }
            //    raw_hlsl.Replace("v0.xyzw = v0.xyzw * tx.xyxy;", "v0.xyzw = v0.xyzw;");
            //    //bpy.Replace("v0.xyzw = v0.xyzw * tx.xyxy;", "v0.xyzw = v0.xyzw;");
            //}
        }
    }

    private bool ConvertInstructions() {
        var texDict = textures.ToDictionary(texture => texture.Index);
        var sortedIndices = texDict.Keys.OrderBy(x => x).ToList();
        var line = hlsl.ReadLine();
        if (line == null)
            return false; // its a broken pixel shader that uses some kind of memory textures
        while (!line.Contains("SV_TARGET2")) {
            line = hlsl.ReadLine();
            if (line == null)
                return false; // its a broken pixel shader that uses some kind of memory textures
        }
        hlsl.ReadLine();
        var splitScript = new StringBuilder();
        var lineNumber = 0;
        do {
            lineNumber++;
            line = hlsl.ReadLine().Trim();
            if (line != null) {
                if (line.Contains("return;"))
                    break;
                //Pre-Process
                if (line.Contains("Sample")) {
                    var equal = line.Split("=")[0];
                    var texIndex = Int32.Parse(line.Split(".Sample")[0].Split("t")[1]);
                    var sampleIndex = Int32.Parse(line.Split("(s")[1].Split("_s,")[0]);
                    var sampleUv = line.Split(", ")[1].Split(")")[0];
                    var dotAfter = line.Split(").")[1];
                    // todo add dimension
                    line = $"{equal}= sample({sortedIndices.IndexOf(texIndex)}, {sampleUv}).{dotAfter}";
                } else if (line.Contains("if") || line.Contains("else") || line.Contains("{") || line.Contains("}")) {
                    bpy.AppendLine(line);
                    Console.WriteLine("IF/ELSE");
                    continue;
                }
                if (line.Contains(" = ")) {                    
                    line = line.Trim();
                    line = line[..^1];
                    line = line.Replace("cmp", "-");
                    //Turn conditionals to mix(val1, val2, fac)
                    var adaptedLine = Regex.Replace(line, "(\\S+) \\? (\\S+) \\: (\\S+)", "mix($2, $3, $1)");

                    //Split sections to evaluate
                    var variable = line.Split(" = ")[0];
                    var equalExp = line.Split(" = ")[1];
                    var variableSplit = variable.Split('.');
                    //string[] outputLines = new string[dimensions.Length];

                    var parser = new HLSLParser(lineNumber.ToString(), Hash);
                    bpy.AppendLine($"\n#LINE {lineNumber}: {line}");
                    bpy.Append(parser.parseEquationFull(equalExp, variableSplit[0], variableSplit[1]));

                    //if (dimensions.Length > 1 && !Regex.IsMatch(adaptedLine, @"(\w+)\((.+)\)\.?([x|y|z|w]{0,4})"))
                    //{
                    //    //No functions in line
                    //    string[] ops = equalExp.Split(' ');
                    //    for (int i = 0; i < dimensions.Length; i++)
                    //    {
                    //        string[] splitVar = variable.Split('.');
                    //        string output = $"{splitVar[0]}.{splitVar[1].ElementAt(i)} =";
                    //        foreach (string op in ops)
                    //        {
                    //            if (Regex.IsMatch(op, @"([\w|\[|\]}]+)\.([x|y|z|w]{0,4})"))
                    //            {
                    //                Match match = Regex.Match(op, @"([\w|\[|\]}]+)\.([x|y|z|w]{0,4})");
                    //                char relevantDim = match.Groups[2].Value.ElementAt(i);
                    //                output += $" {match.Groups[1].Value}.{relevantDim}";
                    //            }
                    //            else
                    //            {
                    //                output += $" {op}";
                    //            }
                    //        }
                    //        bpy.AppendLine(output);
                    //    }
                    //}
                    //else if (dimensions.Length == 1 || dimensions.Length == 0)
                    //{
                    //    //Single dimension in line, leave it be
                    //    bpy.AppendLine(adaptedLine);
                    //}
                    //else
                    //{
                    //    //There's a function in the line, gotta deal with that
                    //    //For now just lets it through
                    //    MatchCollection matches = Regex.Matches(adaptedLine, @"(\w+)\((.+)\)\.?([x|y|z|w]{0,4})");
                    //    foreach (Match match in matches)
                    //    {
                    //        string name = match.Groups[1].Value;
                    //    }
                    //    bpy.AppendLine(adaptedLine);
                    //}
                }
            }
        } while (line != null);

        return true;
    }

    private void AddOutputs() {
        string outputString = @"
        ///RT0
        output.BaseColor = o0.xyz; // Albedo
        
        ///RT1

        // Normal
        float3 biased_normal = o1.xyz - float3(0.5, 0.5, 0.5);
        float normal_length = length(biased_normal);
        float3 normal_in_world_space = biased_normal / normal_length;
        normal_in_world_space.z = sqrt(1.0 - saturate(dot(normal_in_world_space.xy, normal_in_world_space.xy)));
        output.Normal = normalize((normal_in_world_space * 2 - 1.5)*0.5 + 0.5);

        // Roughness
        float smoothness = saturate(8 * (normal_length - 0.375));
        output.Roughness = 1 - smoothness;
 
        ///RT2
        output.Metallic = o2.x;
        output.EmissiveColor = (o2.y - 0.5) * 2 * 5 * output.BaseColor;  // the *5 is a scale to make it look good
        output.AmbientOcclusion = o2.y * 2; // Texture AO

        output.OpacityMask = 1;

        return output;
        ";
        bpy.AppendLine(outputString);
    }

    private void WriteFooter(bool bIsVertexShader) {
        bpy.AppendLine("}").AppendLine("};");
        if (!bIsVertexShader)
            bpy.AppendLine("shader s;").AppendLine($"return s.main({String.Join(',', textures.Select(x => x.Variable))},tx);");
    }
}