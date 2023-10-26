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
using Tiger.Schema.Entity;
using System.Collections;
using Newtonsoft.Json.Linq;

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

    public static List<string> StackPop(int pops)
    {
        if (Stack.Count < pops)
        {
            throw new Exception("Not enough elements in the stack to pop.");
        }

        List<string> v = Stack.Skip(Stack.Count - pops).ToList();
        Stack.RemoveRange(Stack.Count - pops, pops);
        return v;
    }

    public void StackPush(string value)
    {
        if (Stack.Count >= Stack.Capacity)
        {
            throw new Exception("Stack is at capacity.");
        }

        Stack.Add(value);
    }

    public string StackTop()
    {
        if (Stack.Count == 0)
        {
            throw new Exception("Stack is empty.");
        }
        string top = Stack[Stack.Count - 1];
        Stack.RemoveAt(Stack.Count - 1);
        return top;
    }

    public void Evaluate(DynamicArray<Vec4> constants)
    {
        foreach ((int _ip, var op) in Opcodes.Select((value, index) => (index, value)))
        {
            switch (op.op)
            {
                case TfxBytecode.Add:
                case TfxBytecode.Add2:
                    var add = StackPop(2);
                    Console.WriteLine($"Add: {add[0]} + {add[1]}");
                    StackPush($"({add[0]} + {add[1]})");
                    break;
                case TfxBytecode.Subtract:
                    var sub = StackPop(2);
                    Console.WriteLine($"Subtract: {sub[0]} - {sub[1]}");
                    StackPush($"({sub[0]} - {sub[1]})");
                    break;
                case TfxBytecode.Multiply:
                case TfxBytecode.Multiply2:
                    var mul = StackPop(2);
                    Console.WriteLine($"Multiply: {mul[0]} * {mul[1]}");
                    StackPush($"({mul[0]} * {mul[1]})");
                    break;
                case TfxBytecode.IsZero: //Divide?
                    //var isZero = StackTop();
                    //Console.WriteLine($"IsZero: {isZero}");
                    //StackPush($"float4({isZero}.x == 0 ? 1 : 0, " +
                    //    $"{isZero}.y == 0 ? 1 : 0, " +
                    //    $"{isZero}.z == 0 ? 1 : 0, " +
                    //    $"{isZero}.w == 0 ? 1 : 0)");

                    var div = StackPop(2);
                    Console.WriteLine($"Divide: {div[0]} / {div[1]}");
                    StackPush($"({div[0]} / {div[1]})");
                    break;
                case TfxBytecode.MultiplyAdd:
                    var mulAdd = StackPop(3);
                    Console.WriteLine($"MultiplyAdd: {mulAdd[0]} * {mulAdd[1]} + {mulAdd[2]}");
                    StackPush($"({mulAdd[0]} * {mulAdd[1]} + {mulAdd[2]})");
                    break;
                case TfxBytecode.Clamp:
                    var clamp = StackPop(3);
                    Console.WriteLine($"Clamp: clamp({clamp[0]}, {clamp[1]}, {clamp[2]})");
                    StackPush($"clamp({clamp[0]}, {clamp[1]}, {clamp[2]})");
                    break;
                case TfxBytecode.Negate:
                    var negate = StackTop();
                    Console.WriteLine($"Negate: -{negate}");
                    StackPush($"-{negate}");
                    break;
                case TfxBytecode.Merge_1_3:
                    var merge = StackPop(2);
                    Console.WriteLine($"Merge_1_3: float4({merge[0]}.x, {merge[1]}.x, {merge[1]}.y, {merge[1]}.z)");
                    StackPush($"float4({merge[0]}.x, {merge[1]}.x, {merge[1]}.y, {merge[1]}.z)");
                    break;
                case TfxBytecode.PushExternInputFloat:
                    var v = GetExtern(((PushExternInputFloatData)op.data).extern_, ((PushExternInputFloatData)op.data).element);
                    Console.WriteLine($"GetExtern {((PushExternInputFloatData)op.data).extern_}, {v.ToString()}");
                    StackPush(v);
                    break;
                case TfxBytecode.PushConstantVec4:
                    var vec = constants[((PushConstantVec4Data)op.data).constant_index].Vec;
                    Console.WriteLine($"PushConstVec4: float4({vec.X}, {vec.Y}, {vec.Z}, {vec.W})");
                    StackPush($"float4({vec.X}, {vec.Y}, {vec.Z}, {vec.W})");
                    break;

                case TfxBytecode.Unk17: //floor?
                    var floor = StackTop();
                    Console.WriteLine($"Floor: floor({floor})");
                    StackPush($"floor({floor})");
                    break;

                //case TfxBytecode.Unk0d: //Divide?
                //    var div = StackPop(2);
                //    Console.WriteLine($"Divide: {div[0]} / {div[1]}");
                //    StackPush($"({div[0]} / {div[1]})");
                //    break;


                case TfxBytecode.StoreToBuffer:
                    Console.WriteLine($"{op.op} : {TfxBytecodeOp.TfxToString(op, constants)}");
                    Temp.Clear();
                    Temp.AddRange(Stack);
                    Stack.Clear();
                    break;
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

    static string GetExtern(TfxExtern extern_, byte element)
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

