using Arithmic;
using Tiger;
using Tiger.Schema;

public class TfxBytecodeInterpreter
{
    public List<TfxData> Opcodes { get; set; }
    public List<string> Stack { get; set; }
    public List<string> Temp { get; set; }

    public TfxBytecodeInterpreter(List<TfxData> opcodes)
    {
        Opcodes = opcodes ?? new List<TfxData>();
        Stack = new(capacity: 64);
        Temp = new(capacity: 16);
    }

    private List<string> StackPop(int pops)
    {
        if (Stack.Count < pops)
        {
            throw new Exception("Not enough elements in the stack to pop.");
        }

        List<string> v = Stack.Skip(Stack.Count - pops).ToList();
        Stack.RemoveRange(Stack.Count - pops, pops);
        return v;
    }

    private void StackPush(string value)
    {
        if (Stack.Count >= Stack.Capacity)
        {
            throw new Exception("Stack is at capacity.");
        }

        Stack.Add(value);
    }

    private string StackTop()
    {
        if (Stack.Count == 0)
        {
            throw new Exception("Stack is empty.");
        }
        string top = Stack[Stack.Count - 1];
        Stack.RemoveAt(Stack.Count - 1);
        return top;
    }

    public Dictionary<int, string> Evaluate(DynamicArray<Vec4> constants, bool print = false)
    {
        Dictionary<int, string> hlsl = new();
        try
        {
            if (print)
                Console.WriteLine($"--------Evaluating Bytecode:");
            foreach ((int _ip, var op) in Opcodes.Select((value, index) => (index, value)))
            {
                if (print)
                    Console.WriteLine($"0x{op.op:X} {op.op} : {TfxBytecodeOp.TfxToString(op, constants)}");
                switch (op.op)
                {
                    case TfxBytecode.Add:
                    case TfxBytecode.Add2:
                        var add = StackPop(2);
                        StackPush($"({add[0]} + {add[1]})");
                        break;
                    case TfxBytecode.Subtract:
                        var sub = StackPop(2);
                        StackPush($"({sub[0]} - {sub[1]})");
                        break;
                    case TfxBytecode.Multiply:
                    case TfxBytecode.Multiply2:
                        var mul = StackPop(2);
                        StackPush($"({mul[0]} * {mul[1]})");
                        break;
                    case TfxBytecode.Divide:
                        var div = StackPop(2);
                        StackPush($"({div[0]} / {div[1]})");
                        break;
                    case TfxBytecode.IsZero:
                        var isZero = StackTop();
                        StackPush($"(float4({isZero}.x == 0 ? 1 : 0, " +
                            $"{isZero}.y == 0 ? 1 : 0, " +
                            $"{isZero}.z == 0 ? 1 : 0, " +
                            $"{isZero}.w == 0 ? 1 : 0))");
                        break;
                    case TfxBytecode.Min:
                        var min = StackPop(2);
                        StackPush($"(min({min[0]}, {min[1]}))");
                        break;
                    case TfxBytecode.Max:
                        var max = StackPop(2);
                        StackPush($"(max({max[0]}, {max[1]}))");
                        break;
                    case TfxBytecode.LessThan: //I dont think I need to do < for each element?
                        var lessThan = StackPop(2);
                        StackPush(LessThan(lessThan[0], lessThan[1]));
                        break;
                    case TfxBytecode.Dot:
                        var dot = StackPop(2);
                        StackPush($"(dot4({dot[0]}, {dot[1]}))");
                        break;
                    case TfxBytecode.Merge_1_3:
                        var merge = StackPop(2);
                        StackPush($"(float4({merge[0]}.x, {merge[1]}.x, {merge[1]}.y, {merge[1]}.z))");
                        break;
                    case TfxBytecode.Merge_2_2:
                        var merge2_2 = StackPop(2);
                        StackPush($"(float4({merge2_2[0]}.x, {merge2_2[0]}.y, {merge2_2[1]}.x, {merge2_2[1]}.y))");
                        break;
                    case TfxBytecode.Merge_3_1:
                        var merge3_1 = StackPop(2);
                        StackPush($"(float4({merge3_1[0]}.x, {merge3_1[0]}.y, {merge3_1[0]}.z, {merge3_1[1]}.x))");
                        break;
                    case TfxBytecode.Cubic:
                        var Unk0f = StackPop(2);
                        StackPush($"((({Unk0f[1]}.xxxx * {Unk0f[0]} + {Unk0f[1]}.yyyy) * ({Unk0f[0]} * {Unk0f[0]}) + ({Unk0f[1]}.zzzz * {Unk0f[0]} + {Unk0f[1]}.wwww)))");
                        break;
                    case TfxBytecode.Lerp:
                        var lerp = StackPop(3);
                        StackPush($"(lerp({lerp[1]}, {lerp[0]}, {lerp[2]}))");
                        break;
                    case TfxBytecode.MultiplyAdd:
                        var mulAdd = StackPop(3);
                        StackPush($"({mulAdd[0]} * {mulAdd[1]} + {mulAdd[2]})");
                        break;
                    case TfxBytecode.Clamp:
                        var clamp = StackPop(3);
                        StackPush($"(clamp({clamp[0]}, {clamp[1]}, {clamp[2]}))");
                        break;
                    case TfxBytecode.Abs:
                        var abs = StackTop();
                        StackPush($"(abs({abs}))");
                        break;
                    case TfxBytecode.Sign:
                        var sign = StackTop();
                        StackPush($"(sign({sign}))");
                        break;
                    case TfxBytecode.Floor:
                        var floor = StackTop();
                        StackPush($"(floor({floor}))");
                        break;
                    case TfxBytecode.Ceil:
                        var ceil = StackTop();
                        StackPush($"(ceil({ceil}))");
                        break;
                    case TfxBytecode.Round:
                        var round = StackTop();
                        //S2 material expressions dont support round, for some reason...
                        StackPush($"(floor({round}+0.5))");
                        break;
                    case TfxBytecode.Frac:
                        var frac = StackTop();
                        StackPush($"(frac({frac}))");
                        break;
                    case TfxBytecode.Negate:
                        var negate = StackTop();
                        StackPush($"(-{negate})");
                        break;
                    case TfxBytecode.VecRotSin:
                        var VecRotSin = StackTop();
                        StackPush(_trig_helper_vector_sin_rotations_estimate(VecRotSin));
                        break;
                    case TfxBytecode.VecRotCos:
                        var VecRotCos = StackTop();
                        StackPush(_trig_helper_vector_cos_rotations_estimate(VecRotCos));
                        break;
                    case TfxBytecode.VecRotSinCos:
                        var VecRotSinCos = StackTop();
                        StackPush(_trig_helper_vector_sin_cos_rotations_estimate(VecRotSinCos));
                        break;
                    case TfxBytecode.PermuteAllX:
                        var permutex = StackTop();
                        StackPush($"({permutex}.xxxx)");
                        break;
                    case TfxBytecode.Permute:
                        var param = ((PermuteData)op.data).fields;
                        var permute = StackTop();
                        StackPush($"({permute}{TfxBytecodeOp.DecodePermuteParam(param)})");
                        break;
                    case TfxBytecode.Saturate:
                        var saturate = StackTop();
                        StackPush($"(saturate({saturate}))");
                        break;
                    case TfxBytecode.Triangle:
                        var Triangle = StackTop();
                        StackPush(bytecode_op_triangle(Triangle));
                        break;
                    case TfxBytecode.Jitter:
                        var Jitter = StackTop();
                        StackPush(bytecode_op_jitter(Jitter));
                        break;
                    case TfxBytecode.Wander:
                        var Wander = StackTop();
                        StackPush($"{bytecode_op_wander(Wander)}");
                        break;
                    case TfxBytecode.Rand:
                        var Rand = StackTop();
                        StackPush(bytecode_op_rand(Rand));
                        break;
                    case TfxBytecode.RandSmooth:
                        var RandSmooth = StackTop();
                        StackPush(bytecode_op_rand_smooth(RandSmooth));
                        break;
                    case TfxBytecode.TransformVec4:
                        var TransformVec4 = StackPop(5);
                        StackPush($"{mul_vec4(TransformVec4)}");
                        break;
                    case TfxBytecode.PushConstantVec4:
                        var vec = constants[((PushConstantVec4Data)op.data).constant_index].Vec;
                        StackPush($"(float4{vec})");
                        break;
                    case TfxBytecode.LerpConstant:
                        var t = StackTop();
                        var a = constants[((LerpConstantData)op.data).constant_start].Vec;
                        var b = constants[((LerpConstantData)op.data).constant_start + 1].Vec;

                        StackPush($"(lerp(float4{a}, float4{b}, {t}))");
                        break;
                    case TfxBytecode.Spline4Const:
                        var X = StackTop();
                        var C3 = $"float4{constants[((Spline4ConstData)op.data).constant_index].Vec}";
                        var C2 = $"float4{constants[((Spline4ConstData)op.data).constant_index + 1].Vec}";
                        var C1 = $"float4{constants[((Spline4ConstData)op.data).constant_index + 2].Vec}";
                        var C0 = $"float4{constants[((Spline4ConstData)op.data).constant_index + 3].Vec}";
                        var threshold = $"float4{constants[((Spline4ConstData)op.data).constant_index + 4].Vec}";

                        StackPush($"{bytecode_op_spline4_const(X, C3, C2, C1, C0, threshold)}");
                        break;
                    case TfxBytecode.UnkLoadConstant: //Replaces the top of the stack instead of pushing?
                        var take = StackTop(); //Just take the top out then push
                        var UnkLoadConstant = constants[((UnkLoadConstantData)op.data).constant_index].Vec;
                        StackPush($"(float4{UnkLoadConstant})");
                        break;
                    case TfxBytecode.PushExternInputFloat:
                        var v = GetExternFloat(((PushExternInputFloatData)op.data).extern_, ((PushExternInputFloatData)op.data).element * 4);
                        StackPush(v);
                        break;
                    case TfxBytecode.PushExternInputVec4:
                        var PushExternInputVec4 = GetExternVec4(((PushExternInputVec4Data)op.data).extern_, ((PushExternInputVec4Data)op.data).element * 16);
                        StackPush(PushExternInputVec4);
                        break;
                    case TfxBytecode.PushExternInputMat4:
                        //var Mat4 = Matrix4x4.Identity;
                        StackPush($"float4(1,0,0,0)");
                        StackPush($"float4(0,1,0,0)");
                        StackPush($"float4(0,0,1,0)");
                        StackPush($"float4(0,0,0,1)");
                        break;
                    case TfxBytecode.PushExternInputTextureView:
                        //var PushExternInputU64 = GetExtern(((PushExternInputU64Data)op.data).extern_, ((PushExternInputU64Data)op.data).element);
                        StackPush($"float4(1, 1, 1, 1)");
                        break;
                    case TfxBytecode.PushExternInputUav:
                        //var PushExternInputUav = GetExtern(((PushExternInputUavData)op.data).extern_, ((PushExternInputUavData)op.data).element);
                        StackPush($"float4(1, 1, 1, 1)");
                        break;

                    // Global channel stuff
                    case TfxBytecode.Unk4c:
                        var global_channel = GlobalChannelDefaults.GetGlobalChannelDefaults()[((Unk4cData)op.data).unk1];
                        StackPush($"float4{global_channel}");
                        break;
                    case TfxBytecode.PushGlobalChannelVector:
                        global_channel = GlobalChannelDefaults.GetGlobalChannelDefaults()[((PushGlobalChannelVectorData)op.data).unk1];
                        StackPush($"float4{global_channel}");
                        break;
                    case TfxBytecode.Unk50:
                        global_channel = GlobalChannelDefaults.GetGlobalChannelDefaults()[((Unk50Data)op.data).unk1];
                        StackPush($"float4{global_channel}");
                        break;
                    case TfxBytecode.Unk52:
                        global_channel = GlobalChannelDefaults.GetGlobalChannelDefaults()[((Unk52Data)op.data).unk1];
                        StackPush($"float4{global_channel}");
                        break;
                    case TfxBytecode.Unk53:
                        global_channel = GlobalChannelDefaults.GetGlobalChannelDefaults()[((Unk53Data)op.data).unk1];
                        StackPush($"float4{global_channel}");
                        break;
                    case TfxBytecode.Unk54:
                        global_channel = GlobalChannelDefaults.GetGlobalChannelDefaults()[((Unk54Data)op.data).unk1];
                        StackPush($"float4{global_channel}");
                        break;
                    /////

                    case TfxBytecode.SetShaderSampler:
                        v = StackTop();
                        break;
                    case TfxBytecode.PushSampler:
                        StackPush($"float4(0,0,0,0)");
                        break;
                    case TfxBytecode.PushObjectChannelVector:
                        StackPush($"float4(1, 1, 1, 1)");
                        break;
                    case TfxBytecode.PushFromOutput:
                        StackPush($"{hlsl[((PushFromOutputData)op.data).element]}");
                        break;
                    case TfxBytecode.PopOutput:
                        //Temp.AddRange(Stack);

                        if (print)
                            Console.WriteLine($"----Output Stack Count: {Stack.Count}\n");

                        if (Stack.Count == 0) //Shouldnt happen
                            hlsl.TryAdd(((PopOutputData)op.data).slot, "float4(0, 0, 0, 0)");
                        else if (Stack.Count > 1) //Shouldnt happen
                            hlsl.TryAdd(((PopOutputData)op.data).slot, StackTop());
                        else
                            hlsl.TryAdd(((PopOutputData)op.data).slot, StackTop());

                        Stack.Clear(); //Does this matter?
                        break;
                    case TfxBytecode.PopOutputMat4: //uhhhhh, im 100% doing this wrong
                        var PopOutputMat4 = StackPop(4);
                        var Mat4_1 = PopOutputMat4[0];
                        var Mat4_2 = PopOutputMat4[1];
                        var Mat4_3 = PopOutputMat4[2];
                        var Mat4_4 = PopOutputMat4[3];

                        hlsl.TryAdd(((PopOutputMat4Data)op.data).slot, Mat4_1);
                        hlsl.TryAdd(((PopOutputMat4Data)op.data).slot + 1, Mat4_2);
                        hlsl.TryAdd(((PopOutputMat4Data)op.data).slot + 2, Mat4_3);
                        hlsl.TryAdd(((PopOutputMat4Data)op.data).slot + 3, Mat4_4);
                        Stack.Clear();
                        break;
                    case TfxBytecode.PushTemp:
                        var PushTemp = ((PushTempData)op.data).slot;
                        StackPush(Temp[PushTemp]);
                        break;
                    case TfxBytecode.PopTemp:
                        var PopTemp = ((PopTempData)op.data).slot;
                        var PopTemp_v = StackTop();
                        Temp.Insert(PopTemp, PopTemp_v);
                        break;

                    default:
                        if (print)
                            Console.WriteLine($"Not Implemented: {op.op}");
                        break;

                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
        }

        return hlsl;
    }

    private string GetExternFloat(TfxExtern extern_, int element)
    {
        switch (extern_)
        {
            case TfxExtern.Frame:
                switch (element)
                {
                    case 0:
                        return $"(Time)"; // game_time
                    case 0x04:
                        return $"(Time)"; // render_time
                    case 0x14:
                        return $"(0.016)"; // delta_game_time
                    case 0x1C:
                        return $"(16)"; // exposure_scale

                    default:
                        Log.Error($"Unsupported element {element} (0x{(element):X}) for extern {extern_}");
                        return $"(1)";
                }
            case TfxExtern.Atmosphere:
                switch (element)
                {
                    case 0x70:
                        return $"(exists(AtmosTimeOfDay) ? (AtmosTimeOfDay) : (0.5))";
                    case 0x198:
                    case 0x170:
                        return $"(0.0001)";
                    case 0x1b4:
                        return $"(exists(AtmosRotation) ? (AtmosRotation) : (0))";
                    case 0x1b8:
                        return $"(exists(AtmosTimeOfDay) ? (AtmosTimeOfDay) : (1))";
                    case 0x1bc:
                        return $"(0.5)";
                    case 0x1e8:
                        return $"(0)";
                    default:
                        Log.Error($"Unsupported element {element} (0x{(element):X}) for extern {extern_} ");
                        return $"(1)";
                }
            default:
                Log.Error($"Unsupported extern {extern_}[{element} (0x{(element):X})]");
                return $"(1)";
        }
    }

    private string GetExternVec4(TfxExtern extern_, int element)
    {
        switch (extern_)
        {
            case TfxExtern.Deferred:
                switch (element)
                {
                    case 0:
                        return $"float4(0.0, 100, 0.0, 0.0)";
                    default:
                        Log.Error($"Unsupported element {element} (0x{(element):X}) for extern {extern_}");
                        return $"float4(1,1,1,1)";
                }
            case TfxExtern.Frame:
                switch (element)
                {
                    case 0x1A0:
                        return $"float4(0, 0, 0, 0)";
                    case 0x1C0:
                        return $"float4(1, 1, 0, 1)";
                    default:
                        Log.Error($"Unsupported element {element} (0x{(element):X}) for extern {extern_}");
                        return $"float4(1,1,1,1)";
                }
            case TfxExtern.Atmosphere:
                switch (element)
                {
                    case 0x110:
                        return $"float4(0,0,-1.5,0)";
                    case 0x140:
                        return $"float4(0,0,0,0)";
                    case 0x1D0:
                        return $"float4(0,0,0,0)";
                    default:
                        Log.Error($"Unsupported element {element} (0x{(element):X}) for extern {extern_} ");
                        return $"float4(1,1,1,1)";
                }
            default:
                Log.Error($"Unsupported extern {extern_}[{element} (0x{(element):X})]");
                return $"float4(1, 1, 1, 1)";
        }
    }

    private string bytecode_op_spline4_const(string X, string C3, string C2, string C1, string C0, string thresholds)
    {
        string high = $"({C3}*{X}+{C2})"; // C3 * X + C2;
        string low = $"({C1}*{X}+{C0})"; // C1 * X + C0;
        string X2 = $"({X}*{X})"; // X * X;
        string evaluated_spline = $"({high}*{X2}+{low})"; // high * X2 + low;

        string threshold_mask = $"step({thresholds}, {X})";
        string a = $"({_fake_bitwise_ops_fake_xor(threshold_mask, $"({threshold_mask}).yzww")})";
        string channel_mask = $"float4(({a}).x, ({a}).y, ({a}).z, ({threshold_mask}).w)"; // float4(_fake_bitwise_ops_fake_xor(threshold_mask, threshold_mask.yzww).xyz, threshold_mask.w);
        string spline_result_in_4 = $"({evaluated_spline} * {channel_mask})"; // evaluated_spline * channel_mask;
        string spline_result = $"(({spline_result_in_4}).x + ({spline_result_in_4}).y + ({spline_result_in_4}).z + ({spline_result_in_4}).w)"; // spline_result_in_4.x + spline_result_in_4.y + spline_result_in_4.z + spline_result_in_4.w;

        return $"(({spline_result}).xxxx)"; // spline_result.xxxx;
    }

    private string _fake_bitwise_ops_fake_xor(string a, string b)
    {
        return $"{fmod($"({a}+{b})", "2")}";
    }

    private string fmod(string x, string y)
    {
        return $"({x}-floor({x}/{y})*{y})";
    }

    private string bytecode_op_triangle(string x)
    {
        string wrapped = $"({x}-floor({x}+0.5))";   // wrap to [-0.5, 0.5] range
        string abs_wrap = $"(abs({wrapped}))";      // abs turns into triangle wave between [0, 0.5]
        string triangle_result = $"({abs_wrap}*2)"; // scale to [0, 1] range

        return triangle_result;
    }

    private string bytecode_op_jitter(string x)
    {
        string rotations = $"({x}.xxxx * float4(4.67, 2.99, 1.08, 1.35) + float4(0.52, 0.37, 0.16, 0.79))";
        string a = $"({rotations} - floor({rotations} + 0.5))";
        string ma = $"(abs({a}) * -16.0 + 8.0)";
        string sa = $"({a} * 0.25)";
        string v = $"(dot4({sa}, {ma}) + 0.5)";
        string v2 = $"({v}*{v})";
        string jitter_result = $"((-2.0 * {v} + 3.0) * {v2})";
        return jitter_result;
    }

    private string bytecode_op_wander(string x)
    {
        string rot0 = $"({x}.xxxx * float4(4.08, 1.02, 3.0 / 5.37, 3.0 / 9.67) + float4(0.92, 0.33, 0.26, 0.54))";
        string rot1 = $"({x}.xxxx * float4(1.83, 3.09, 0.39, 0.87) + float4(0.12, 0.37, 0.16, 0.79))";
        string sines0 = $"{_trig_helper_vector_pseudo_sin_rotations(rot0)}";
        string sines1 = $"({_trig_helper_vector_pseudo_sin_rotations(rot1)} * float4(0.02, 0.02, 0.28, 0.28))";
        string wander_result = $"(0.5 + dot4({sines0}, {sines1})).xxxx";

        return wander_result;
    }

    private string bytecode_op_rand(string x)
    {
        string v0 = $"(floor({x}.x))";
        string val0 = $"(frac(dot4({v0}, float4(1.0 / 1.043501, 1.0 / 0.794471, 1.0 / 0.113777, 1.0 / 0.015101))))";
        val0 = $"(frac({val0} * {val0} * 251.0))";
        return val0;
    }

    private string bytecode_op_rand_smooth(string X)
    {
        string v = $"({X}.x)";
        string v0 = $"(floor({v}+0.5))";
        string v1 = $"({v0}+1.0)";
        string f = $"({v} - {v0})";
        string f2 = $"({f} * {f})";

        // hermite smooth interpolation (3*f^2 - 2*f^3)
        string smooth_f = $"((-2.0 * {f} + 3.0) * {f2})";

        // these magic numbers are 1/(prime/1000000)
        string val0 = $"(dot4({v0}.xxxx, float4(1.0 / 1.043501, 1.0 / 0.794471, 1.0 / 0.113777, 1.0 / 0.015101)))";
        string val1 = $"(dot4({v1}.xxxx, float4(1.0 / 1.043501, 1.0 / 0.794471, 1.0 / 0.113777, 1.0 / 0.015101)))";

        val0 = $"(frac({val0}))";
        val1 = $"(frac({val1}))";

        //			val0=	bbs(val0);		// Blum-Blum-Shub randomimzer
        val0 =
        val0 = $"(frac({val0}))";

        //			val10=	bbs(val1);		// Blum-Blum-Shub randomimzer
        val1 = $"({val1} * {val1} * 251.0)";
        val1 = $"(frac({val1}))";

        string rand_smooth_result = $"(lerp({val0}, {val1}, {smooth_f}).xxxx)";

        return rand_smooth_result;
    }

    private string mul_vec4(List<string> TransformVec4) //probably wrong
    {
        var x_axis = TransformVec4[0];
        var y_axis = TransformVec4[1];
        var z_axis = TransformVec4[2];
        var w_axis = TransformVec4[3];
        var value = TransformVec4[4];

        string res = $"({x_axis}*{value}.xxxx)";  //x_axis.mul(rhs.xxxx());
        res = $"({res}+({y_axis}*{value}.yyyy))"; //res = res.add(self.y_axis.mul(rhs.yyyy()));
        res = $"({res}+({z_axis}*{value}.zzzz))"; //res = res.add(self.z_axis.mul(rhs.zzzz()));
        res = $"({res}+({w_axis}*{value}.wwww))"; //res = res.add(self.w_axis.mul(rhs.wwww()));

        return res;
    }

    private string GreaterThan(string a, string b)
    {
        return $"({a} > {b})";
    }

    private string LessThan(string a, string b)
    {
        return $"({a} < {b})";
    }

    private string _trig_helper_vector_sin_rotations_estimate_clamped(string a)
    {
        string y = $"({a}*(-16*abs({a})+8))";
        return $"({y}*(0.225*abs({y})+0.775))";
    }

    private string _trig_helper_vector_sin_rotations_estimate(string a)
    {
        string w = $"({a}-(floor({a}+0.5)))"; // wrap to [-0.5, 0.5] range
        return _trig_helper_vector_sin_rotations_estimate_clamped(w);
    }

    private string _trig_helper_vector_sin_cos_rotations_estimate(string a)
    {
        return _trig_helper_vector_sin_rotations_estimate($"({a}+float4(0.0, 0.25, 0.0, 0.25))");
    }

    private string _trig_helper_vector_cos_rotations_estimate(string a)
    {
        return _trig_helper_vector_sin_rotations_estimate($"({a}+0.25)");
    }

    //pseudo
    private string _trig_helper_vector_pseudo_sin_rotations(string a)
    {
        string w = $"({a}-floor({a} + 0.5))";
        return _trig_helper_vector_pseudo_sin_rotations_clamped(w);
    }

    private string _trig_helper_vector_pseudo_sin_rotations_clamped(string a)
    {
        return $"({a} * (abs({a}) * -16.0 + 8.0))";
    }
}
