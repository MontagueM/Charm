using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Tiger.Schema;
using Tiger;

using Vector4 = System.Numerics.Vector4;
using Arithmic;

public class TfxBytecodeInterpreter
{
    public static List<TfxData> Opcodes { get; set; }
    public static List<string> Stack { get; set; }
    public static List<string> Temp { get; set; }

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

    public Dictionary<int, string> Evaluate(DynamicArray<Vec4> constants)
    {
        Dictionary<int, string> hlsl = new();
        try
        {
            Console.WriteLine($"--------Evaluating Bytecode:");
            foreach ((int _ip, var op) in Opcodes.Select((value, index) => (index, value)))
            {
                Console.WriteLine($"{op.op} : {TfxBytecodeOp.TfxToString(op, constants)}");
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
                    case TfxBytecode.Unk0e: //Not correct
                        var Unk0e = StackPop(2);
                        StackPush($"(({Unk0e[0]} + {Unk0e[1]}) / 2.0)");
                        break;
                    case TfxBytecode.Unk0f:
                        var Unk0f = StackPop(2);
                        StackPush($"((({Unk0f[1]}.xxxx * {Unk0f[0]} + {Unk0f[1]}.yyyy) * ({Unk0f[0]} * {Unk0f[0]})) + ({Unk0f[1]}.zzzz * {Unk0f[0]} + {Unk0f[1]}.wwww))");
                        break;
                    case TfxBytecode.Unk10: //Im gonna call this SubMAD
                        var unk10 = StackPop(3);
                        string v33 = $"({unk10[1]}-{unk10[0]})";
                        StackPush($"(({v33}*{unk10[2]})+{unk10[0]})");
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
                        StackPush($"(float4({vec.X}, {vec.Y}, {vec.Z}, {vec.W}))");
                        break;
                    case TfxBytecode.Unk35:
                        var Unk35 = StackTop();
                        var v1 = constants[((Unk35Data)op.data).constant_start].Vec;
                        var v2 = constants[((Unk35Data)op.data).constant_start + 1].Vec;

                        StackPush($"(((float4{v2}-float4{v1})*{Unk35})+float4{v1})");
                        break;
                    case TfxBytecode.UnkLoadConstant: //Replaces the top of the stack instead of pushing?
                        var take = StackTop(); //Just take the top out then push
                        var UnkLoadConstant = constants[((UnkLoadConstantData)op.data).constant_index].Vec;
                        StackPush($"(float4({UnkLoadConstant.X}, {UnkLoadConstant.Y}, {UnkLoadConstant.Z}, {UnkLoadConstant.W}))");
                        break;
                    case TfxBytecode.PushExternInputFloat:
                        var v = GetExtern(((PushExternInputFloatData)op.data).extern_, ((PushExternInputFloatData)op.data).element);
                        StackPush(v);
                        break;
                    case TfxBytecode.PushExternInputVec4:
                        var PushExternInputVec4 = GetExtern(((PushExternInputVec4Data)op.data).extern_, ((PushExternInputVec4Data)op.data).element);
                        StackPush(PushExternInputVec4);
                        break;
                    case TfxBytecode.PushExternInputMat4:
                        //var Mat4 = Matrix4x4.Identity;
                        StackPush($"(float4(1,0,0,0))");
                        StackPush($"(float4(0,1,0,0))");
                        StackPush($"(float4(0,0,1,0))");
                        StackPush($"(float4(0,0,0,1))");
                        break;
                    case TfxBytecode.PushExternInputU64:
                        var PushExternInputU64 = GetExtern(((PushExternInputU64Data)op.data).extern_, ((PushExternInputU64Data)op.data).element);
                        StackPush(PushExternInputU64);
                        break;
                    case TfxBytecode.PushExternInputU64Unknown:
                        var PushExternInputU64Unknown = GetExtern(((PushExternInputU64UnknownData)op.data).extern_, ((PushExternInputU64UnknownData)op.data).element);
                        StackPush(PushExternInputU64Unknown);
                        break;

                    case TfxBytecode.Unk4c:
                    case TfxBytecode.PushObjectChannelVector:
                    case TfxBytecode.Unk4e:
                    case TfxBytecode.Unk4f:
                    case TfxBytecode.Unk50:
                    case TfxBytecode.Unk52:
                    case TfxBytecode.Unk53:
                    case TfxBytecode.Unk54:
                        StackPush($"(float4(1, 1, 1, 1))");
                        break;
                    case TfxBytecode.PushFromOutput:
                        StackPush($"{hlsl[((PushFromOutputData)op.data).element]}");
                        break;
                    case TfxBytecode.PopOutput:
                        //Temp.AddRange(Stack);

                        if(Stack.Count == 0 || Stack.Count > 1) //Shouldnt happen
                            hlsl.TryAdd(((PopOutputData)op.data).slot, "float4(1, 1, 1, 1)");
                        else
                            hlsl.TryAdd(((PopOutputData)op.data).slot, StackTop());

                        Stack.Clear();
                        break;
                    case TfxBytecode.PopOutputMat4: //uhhhhh, im 100% doing this wrong
                        var PopOutputMat4 = StackPop(4);
                        var Mat4_1 = PopOutputMat4[0];
                        var Mat4_2 = PopOutputMat4[1];
                        var Mat4_3 = PopOutputMat4[2];
                        var Mat4_4 = PopOutputMat4[3];

                        hlsl.TryAdd(((PopOutputMat4Data)op.data).slot, Mat4_1);
                        hlsl.TryAdd(((PopOutputMat4Data)op.data).slot+1, Mat4_2);
                        hlsl.TryAdd(((PopOutputMat4Data)op.data).slot+2, Mat4_3);
                        hlsl.TryAdd(((PopOutputMat4Data)op.data).slot+3, Mat4_4);
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

    private string GetExtern(TfxExtern extern_, byte element, bool asFloat = false)
    {
        switch (extern_)
        {
            case TfxExtern.Frame:
                switch (element)
                {
                    case 0:
                        if(asFloat)
                            return $"float4(Time, Time, Time, Time)";
                        else
                            return $"float4(Time, Time, 1, 1)";
                    case 1:
                        return $"float4(1, 1, 1, 1)"; // Exposure scales
                    case 4:
                        return $"float4(0, 0, 0, 0)"; // Stubbed
                    default:
                        Log.Error($"Unsupported element {element} for extern {extern_}");
                        return $"float4(0, 0, 0, 0)";
                }
            default:
                Log.Error($"Unsupported extern {extern_}[{element}]");
                return $"float4(1, 1, 1, 1)";
        }
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
        string rot0 = $"({x}.xxxx * float4(4.08, 1.02, 3.0 / 5.37, 3.0 / 9.67))";
        string rot1 = $"({x}.xxxx * float4(1.83, 3.09, 0.39, 0.87) + float4(0.12, 0.37, 0.16, 0.79))";
        string sines0 = $"({_trig_helper_vector_pseudo_sin_rotations(rot0)})";
        string sines1 =  $"({_trig_helper_vector_pseudo_sin_rotations(rot1)} * float4(0.02, 0.02, 0.28, 0.28))";
        string wander_result = $"(0.5 + dot4({sines0}, {sines1}))"; 

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

