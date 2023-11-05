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
            foreach ((int _ip, var op) in Opcodes.Select((value, index) => (index, value)))
            {
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

                    case TfxBytecode.Unk0d: //??????
                        var Unk0d = StackPop(2);
                        StackPush($"(((float4({Unk0d[1]}.x, {Unk0d[1]}.y, 0.0, 0.0) && float4(0.0, 0.0, 0.0, 0.0)) || 1.0) + ((float4(0.0, 0.0, {Unk0d[0]}.z, {Unk0d[0]}.w) && float4(0.0, 0.0, 0.0, 0.0)) || 1.0))");
                        break;

                    case TfxBytecode.Unk0f: //jesus christ
                        var Unk0f = StackPop(2);
                        StackPush($"((dot(float4({Unk0f[1]}.x, {Unk0f[1]}.y, {Unk0f[0]}.x, {Unk0f[0]}.y), float4({Unk0f[0]}.x, {Unk0f[0]}.y, {Unk0f[0]}.x, {Unk0f[0]}.y)) + " +
                            $"dot(float4({Unk0f[1]}.z, {Unk0f[1]}.w, {Unk0f[0]}.x, {Unk0f[0]}.y), float4({Unk0f[0]}.x, {Unk0f[0]}.y, {Unk0f[0]}.x, {Unk0f[0]}.y))) * dot(float4({Unk0f[0]}.x, {Unk0f[0]}.y, {Unk0f[0]}.x, {Unk0f[0]}.y), float4({Unk0f[0]}.x, {Unk0f[0]}.y, {Unk0f[0]}.x, {Unk0f[0]}.y)) + " +
                            $"(dot(float4({Unk0f[1]}.y, {Unk0f[1]}.x, {Unk0f[0]}.x, {Unk0f[0]}.y), float4({Unk0f[0]}.x, {Unk0f[0]}.y, {Unk0f[0]}.x, {Unk0f[0]}.y)) + dot(float4({Unk0f[1]}.w, {Unk0f[1]}.z, {Unk0f[0]}.x, {Unk0f[0]}.y), float4({Unk0f[0]}.x, {Unk0f[0]}.y, {Unk0f[0]}.x, {Unk0f[0]}.y))))");
                        break;

                    case TfxBytecode.Unk1a: //chatgpt probably messing all this up...
                        var Unk1a = StackTop();
                        StackPush($"({Unk1a} - ((floor({Unk1a}) - step({Unk1a}, floor({Unk1a}))) && step(8388608.0, {Unk1a})))");
                        break;

                    //case TfxBytecode.Unk1a: //frac?, is this one right?
                    //    var frac = StackTop();
                    //    StackPush($"frac({frac})");
                    //    break;

                    case TfxBytecode.Unk27: //bytecode_op_triangle
                        var Unk27 = StackTop();
                        StackPush($"(abs({Unk27} - round({Unk27})) * 2.0)");
                        break;

                    case TfxBytecode.Unk3d:
                    case TfxBytecode.Unk3f:
                    case TfxBytecode.Unk4c:
                    //case TfxBytecode.Unk4d:
                    case TfxBytecode.Unk4e:
                    case TfxBytecode.Unk4f:
                        StackPush($"(float4(1, 1, 1, 1))");
                        break;

                    case TfxBytecode.PushExternInputFloat:
                        var v = GetExtern(((PushExternInputFloatData)op.data).extern_, ((PushExternInputFloatData)op.data).element);
                        StackPush(v);
                        break;
                    case TfxBytecode.PushConstantVec4:
                        var vec = constants[((PushConstantVec4Data)op.data).constant_index].Vec;
                        StackPush($"(float4({vec.X}, {vec.Y}, {vec.Z}, {vec.W}))");
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
                        Console.WriteLine($"{op.op} : {TfxBytecodeOp.TfxToString(op, constants)}");
                        Temp.Clear();
                        Temp.AddRange(Stack);
                        Stack.Clear();

                        //Just output 1 to the given buffer for the time being
                        hlsl.TryAdd(((PopOutputData)op.data).unk1, "float4(1, 1, 0, 0)");
                        break;
                    //case TfxBytecode.StoreToBuffer:
                    //    Console.WriteLine($"{op.op} : {TfxBytecodeOp.TfxToString(op, constants)}");
                    //    Temp.Clear();
                    //    Temp.AddRange(Stack);
                    //    Stack.Clear();

                    //    //Just output 1 to the given buffer for the time being
                    //    hlsl.TryAdd(((StoreToBufferData)op.data).element, "float4(1, 1, 0, 0)");
                    //    break;
                    default:
                        Console.WriteLine($"{op.op} : {TfxBytecodeOp.TfxToString(op, constants)}");
                        break;

                }    
            }
            
            foreach (var a in Temp)
            {
                Console.WriteLine($"Stack Length {Temp.Count}, Stack Value {a}");
            }
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
}

