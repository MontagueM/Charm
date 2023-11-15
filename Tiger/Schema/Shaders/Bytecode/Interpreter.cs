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
        Stack = new(capacity: 128);
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
                    case TfxBytecode.IsZero: //Divide?
                        var isZero = StackTop();
                        StackPush($"(float4({isZero}.x == 0 ? 1 : 0, " +
                            $"{isZero}.y == 0 ? 1 : 0, " +
                            $"{isZero}.z == 0 ? 1 : 0, " +
                            $"{isZero}.w == 0 ? 1 : 0))");

                        //var div = StackPop(2);
                        //Console.WriteLine($"Divide: {div[0]} / {div[1]}");
                        //StackPush($"({div[0]} / {div[1]})");
                        break;
                    case TfxBytecode.MultiplyAdd:
                        var mulAdd = StackPop(3);
                        StackPush($"({mulAdd[0]} * {mulAdd[1]} + {mulAdd[2]})");
                        break;
                    case TfxBytecode.Clamp:
                        var clamp = StackPop(3);
                        StackPush($"(clamp({clamp[0]}, {clamp[1]}, {clamp[2]}))");
                        break;
                    case TfxBytecode.Cosine:
                        var cos = StackTop();
                        StackPush($"(cos({cos}))");
                        break;
                    case TfxBytecode.Negate:
                        var negate = StackTop();
                        StackPush($"(-{negate})");
                        break;
                    case TfxBytecode.Merge_1_3:
                        var merge = StackPop(2);
                        StackPush($"(float4({merge[0]}.x, {merge[1]}.x, {merge[1]}.y, {merge[1]}.z))");
                        break;

                    //Unk0B

                    case TfxBytecode.Merge_2_2: //??????
                        var merge2_2 = StackPop(2);
                        StackPush($"(float4({merge2_2[0]}.x, {merge2_2[0]}.y, {merge2_2[1]}.x, {merge2_2[1]}.y))");
                        break;

                    case TfxBytecode.Unk0f: //jesus christ
                        var Unk0f = StackPop(2);
                        StackPush($"((dot4({Unk0f[1]}, {Unk0f[0]}) + dot4({Unk0f[1]}.yxwz, {Unk0f[0]}.yxwz)) * dot4({Unk0f[0]}, {Unk0f[0]}) + (dot4({Unk0f[1]}.zwxy, {Unk0f[0]}.zwxy) + dot4({Unk0f[1]}.wzyx, {Unk0f[0]}.wzyx)))");
                        break;

                    case TfxBytecode.Unk1a:
                        var Unk1a = StackTop();
                        StackPush($"(frac({Unk1a}))");
                        break;

                    case TfxBytecode.Unk27: //bytecode_op_triangle
                        var Unk27 = StackTop();
                        StackPush($"(abs({Unk27} - floor({Unk27} + 0.5)) * 2.0)");
                        break;

                    case TfxBytecode.Unk29: //bytecode_op_wander
                        var Unk29 = StackTop();
                        StackPush($"{bytecode_op_wander(Unk29)}");
                        break;

                    //case TfxBytecode.Unk3d:
                    case TfxBytecode.Unk4c:
                    case TfxBytecode.PushObjectChannelVector:
                    case TfxBytecode.Unk4e:
                    case TfxBytecode.Unk4f:
                        StackPush($"(float4(1, 1, 1, 1))");
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
                    case TfxBytecode.UnkLoadConstant:
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
                    case TfxBytecode.Min:
                        var min = StackPop(2);
                        StackPush($"(min({min[0]}, {min[1]}))");
                        break;
                    case TfxBytecode.Max:
                        var max = StackPop(2);
                        StackPush($"(max({max[0]}, {max[1]}))");
                        break;

                    case TfxBytecode.Unk17: //floor?
                        var floor = StackTop();
                        StackPush($"(floor({floor}))");
                        break;

                    case TfxBytecode.PopOutput: //??
                        //Console.WriteLine($"{op.op} : {TfxBytecodeOp.TfxToString(op, constants)}");
                        //Temp.Clear();
                        //Temp.AddRange(Stack);

                        foreach (var a in Stack)
                        {
                            Console.WriteLine($"Stack Length {Stack.Count}, Stack Value {a}");
                        }
                        Stack.Clear();

                        //Just output 1 to the given buffer for the time being
                        hlsl.TryAdd(((PopOutputData)op.data).unk1, "float4(1, 1, 0, 0)");
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
                        //Console.WriteLine($"{op.op} : {TfxBytecodeOp.TfxToString(op, constants)}");
                        break;

                }    
            }
            
            //foreach (var a in Temp)
            //{
            //    Console.WriteLine($"Stack Length {Temp.Count}, Stack Value {a}");
            //}
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
        }

        return hlsl;
    }

    private string GetExtern(TfxExtern extern_, byte element)
    {
        switch (extern_)
        {
            case TfxExtern.Frame:
                switch (element)
                {
                    case 0:
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
                return $"float4(0, 0, 0, 0)";
        }
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

