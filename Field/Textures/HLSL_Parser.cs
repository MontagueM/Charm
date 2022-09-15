﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Field.Textures.Tokens;

namespace Field.Textures
{
    internal class HLSLParser
    {
        public string line_id;
        private int var_index = 0;
        private string createIdentifier()
        {
            var_index++;
            return $"{line_id}_{var_index}";
        }
        //private static void Main(string[] args)
        //{
        //    string inputExp = "cb0[2].z * -r0.x + abs(v3.xy + cb0[3].yz).w";
        //    string inputVar = "r0.z";

        //    IToken tokens = parseEquation(inputExp);
        //    Tuple<string, string> traversal = traverse(tokens);
        //    Console.WriteLine(traversal.Item1);
        //    Console.WriteLine($"variable_dict['{inputVar}'] = {traversal.Item2}");
        //}
        public HLSLParser(string line_id)
        {
            this.line_id = line_id;
        }
        public string parseEquationFull(string equation, string inputVar)
        {
            IToken tree = parseEquation(equation);
            Tuple<string, string> traversal = traverse(tree);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(traversal.Item1);
            stringBuilder.AppendLine($"variable_dict['{inputVar}'] = {traversal.Item2}");
            return stringBuilder.ToString();
        }

        private Tuple<string, string> traverse(IToken tree)
        {
            StringBuilder outputString = new StringBuilder();
            string outputConnector = "";
            if (tree == null)
            {
                return new Tuple<string, string>("", "");
            }
            else if (tree is OperatorToken)
            {

                OperatorToken operatorToken = (OperatorToken)tree;
                List<string> paramConnectors = new List<string>();
                //Operator or Function
                foreach (IToken token in operatorToken.children)
                {
                    Tuple<string, string> tuple = traverse(token);
                    if (tuple.Item1 != "")
                        outputString.AppendLine(tuple.Item1);
                    paramConnectors.Add(tuple.Item2);
                }
                Tuple<string, string> operatorNodeS = operatorNode(operatorToken.operation == "function" ? ((FunctionOperatorToken)operatorToken).FunctionName : operatorToken.operation, paramConnectors.ToArray());
                if (operatorNodeS.Item1 != "")
                    outputString.AppendLine(operatorNodeS.Item1);
                outputConnector = operatorNodeS.Item2;
            }
            else
            {
                if (tree is FloatValueToken)
                {
                    ///Value
                    string name = createIdentifier();
                    outputString.AppendLine(@$"{name} = matnodes.new(""ShaderNodeValue"")");
                    outputString.AppendLine(@$"{name}.outputs[0].default_value = {tree.ToString()}");
                    outputConnector = $@"{name}.outputs[0]";
                }
                else
                {
                    ///Variable
                    outputConnector = $"variable_dict['{tree.ToString()}']";
                }

            }
            return new Tuple<string, string>(outputString.ToString(), outputConnector);
        }
        private Tuple<string, string> operatorNode(string operation, string[] paramConnectors)
        {
            StringBuilder outputScript = new StringBuilder();
            string outputConnector = "";
            string name = createIdentifier();
            switch (operation)
            {
                case "~":
                    outputScript.AppendLine($@"{name} = matnodes.new(""ShaderNodeMath"")");
                    outputScript.AppendLine($@"{name}.operation = 'MULTIPLY'");
                    outputScript.AppendLine($@"{name}.inputs[0].default_value = -1.0");
                    outputScript.AppendLine($@"link({paramConnectors[0]}, {name}.inputs[1])");
                    outputConnector = $@"{name}.outputs[0]";
                    break;
                case "`":
                    outputConnector = paramConnectors[0];
                    break;
                case "*":
                    outputScript.AppendLine($@"{name} = matnodes.new(""ShaderNodeMath"")");
                    outputScript.AppendLine($@"{name}.operation = 'MULTIPLY'");
                    outputScript.AppendLine($@"link({paramConnectors[0]}, {name}.inputs[0])");
                    outputScript.AppendLine($@"link({paramConnectors[1]}, {name}.inputs[1])");
                    outputConnector = $@"{name}.outputs[0]";
                    break;
                case "/":
                    outputScript.AppendLine($@"{name} = matnodes.new(""ShaderNodeMath"")");
                    outputScript.AppendLine($@"{name}.operation = 'DIVIDE'");
                    outputScript.AppendLine($@"link({paramConnectors[0]}, {name}.inputs[0])");
                    outputScript.AppendLine($@"link({paramConnectors[1]}, {name}.inputs[1])");
                    outputConnector = $@"{name}.outputs[0]";
                    break;
                case "%":
                    outputScript.AppendLine($@"{name} = matnodes.new(""ShaderNodeMath"")");
                    outputScript.AppendLine($@"{name}.operation = 'MODULO'");
                    outputScript.AppendLine($@"link({paramConnectors[0]}, {name}.inputs[0])");
                    outputScript.AppendLine($@"link({paramConnectors[1]}, {name}.inputs[1])");
                    outputConnector = $@"{name}.outputs[0]";
                    break;
                case "+":
                    outputScript.AppendLine($@"{name} = matnodes.new(""ShaderNodeMath"")");
                    outputScript.AppendLine($@"{name}.operation = 'ADD'");
                    outputScript.AppendLine($@"link({paramConnectors[0]}, {name}.inputs[0])");
                    outputScript.AppendLine($@"link({paramConnectors[1]}, {name}.inputs[1])");
                    outputConnector = $@"{name}.outputs[0]";
                    break;
                case "-":
                    outputScript.AppendLine($@"{name} = matnodes.new(""ShaderNodeMath"")");
                    outputScript.AppendLine($@"{name}.operation = 'SUBTRACT'");
                    outputScript.AppendLine($@"link({paramConnectors[0]}, {name}.inputs[0])");
                    outputScript.AppendLine($@"link({paramConnectors[1]}, {name}.inputs[1])");
                    outputConnector = $@"{name}.outputs[0]";
                    break;
                case "saturate":
                    //Clamp defaults are fine
                    outputScript.AppendLine($@"{name} = matnodes.new(""ShaderNodeClamp"")");
                    outputScript.AppendLine($@"link({paramConnectors[0]}, {name}.inputs[0])");
                    outputConnector = $@"{name}.outputs[0]";
                    break;
                case "mix":
                    outputScript.AppendLine($@"{name} = matnodes.new(""ShaderNodeMixRGB"")");
                    outputScript.AppendLine($@"link({paramConnectors[0]}, {name}.inputs[0])");
                    outputScript.AppendLine($@"link({paramConnectors[1]}, {name}.inputs[1])");
                    outputConnector = $@"{name}.outputs[0]";
                    break;
                //case "ddx_coarse":
                //case "ddy_coarse":
                case "sqrt":
                    outputScript.AppendLine($@"{name} = matnodes.new(""ShaderNodeMath"")");
                    outputScript.AppendLine($@"{name}.operation = 'SQRT'");
                    outputScript.AppendLine($@"link({paramConnectors[0]}, {name}.inputs[0])");
                    outputConnector = $@"{name}.outputs[0]";
                    break;
                case "floor":
                    outputScript.AppendLine($@"{name} = matnodes.new(""ShaderNodeMath"")");
                    outputScript.AppendLine($@"{name}.operation = 'FLOOR'");
                    outputScript.AppendLine($@"link({paramConnectors[0]}, {name}.inputs[0])");
                    outputConnector = $@"{name}.outputs[0]";
                    break;
                case "frac":
                    outputScript.AppendLine($@"{name} = matnodes.new(""ShaderNodeMath"")");
                    outputScript.AppendLine($@"{name}.operation = 'FRACT'");
                    outputScript.AppendLine($@"link({paramConnectors[0]}, {name}.inputs[0])");
                    outputConnector = $@"{name}.outputs[0]";
                    break;
                case "max":
                    outputScript.AppendLine($@"{name} = matnodes.new(""ShaderNodeMath"")");
                    outputScript.AppendLine($@"{name}.operation = 'MAX'");
                    outputScript.AppendLine($@"link({paramConnectors[0]}, {name}.inputs[0])");
                    outputScript.AppendLine($@"link({paramConnectors[1]}, {name}.inputs[1])");
                    outputConnector = $@"{name}.outputs[0]";
                    break;
                case "min":
                    outputScript.AppendLine($@"{name} = matnodes.new(""ShaderNodeMath"")");
                    outputScript.AppendLine($@"{name}.operation = 'MIN'");
                    outputScript.AppendLine($@"link({paramConnectors[0]}, {name}.inputs[0])");
                    outputScript.AppendLine($@"link({paramConnectors[1]}, {name}.inputs[1])");
                    outputConnector = $@"{name}.outputs[0]";
                    break;
                case "log2":
                    outputScript.AppendLine($@"{name} = matnodes.new(""ShaderNodeMath"")");
                    outputScript.AppendLine($@"{name}.operation = 'LOGARITHM'");
                    outputScript.AppendLine($@"link({paramConnectors[0]}, {name}.inputs[0])");
                    outputScript.AppendLine($@"{name}.inputs[1].default_value = 2.0");
                    outputConnector = $@"{name}.outputs[0]";
                    break;
                case "log10":
                    outputScript.AppendLine($@"{name} = matnodes.new(""ShaderNodeMath"")");
                    outputScript.AppendLine($@"{name}.operation = 'LOGARITHM'");
                    outputScript.AppendLine($@"link({paramConnectors[0]}, {name}.inputs[0])");
                    outputScript.AppendLine($@"{name}.inputs[1].default_value = 10.0");
                    outputConnector = $@"{name}.outputs[0]";
                    break;
                case "log":
                    outputScript.AppendLine($@"{name} = matnodes.new(""ShaderNodeMath"")");
                    outputScript.AppendLine($@"{name}.operation = 'LOGARITHM'");
                    outputScript.AppendLine($@"link({paramConnectors[0]}, {name}.inputs[0])");
                    outputScript.AppendLine($@"{name}.inputs[1].default_value = 2.71828 #Euler's Number");
                    outputConnector = $@"{name}.outputs[0]";
                    break;
                case "exp":
                    outputScript.AppendLine($@"{name} = matnodes.new(""ShaderNodeMath"")");
                    outputScript.AppendLine($@"{name}.operation = 'POWER'");
                    outputScript.AppendLine($@"{name}.inputs[0].default_value = 2.71828 #Euler's Number");
                    outputScript.AppendLine($@"link({paramConnectors[0]}, {name}.inputs[1])");
                    outputConnector = $@"{name}.outputs[0]";
                    break;
                case "exp2":
                    outputScript.AppendLine($@"{name} = matnodes.new(""ShaderNodeMath"")");
                    outputScript.AppendLine($@"{name}.operation = 'POWER'");
                    outputScript.AppendLine($@"{name}.inputs[0].default_value = 2.0");
                    outputScript.AppendLine($@"link({paramConnectors[0]}, {name}.inputs[1])");
                    outputConnector = $@"{name}.outputs[0]";
                    break;
                case "abs":
                    outputScript.AppendLine($@"{name} = matnodes.new(""ShaderNodeMath"")");
                    outputScript.AppendLine($@"{name}.operation = 'ABSOLUTE'");
                    outputScript.AppendLine($@"link({paramConnectors[0]}, {name}.inputs[0])");
                    outputConnector = $@"{name}.outputs[0]";
                    break;
                default:
                    Console.WriteLine($"#DON'T KNOW HOW DO TO OPERATION {operation}");
                    outputConnector = paramConnectors[0];
                    break;
            }
            return Tuple.Create(outputScript.ToString(), outputConnector);
        }
        private IToken parseEquation(string inputExp)
        {
            List<IToken> outputStack = new List<IToken>();
            Stack<OperatorToken> operatorStack = new Stack<OperatorToken>();

            foreach (string _s in splitParams(inputExp, " "))
            {
                string s = _s;
                if (OperatorToken.precedence.Keys.Contains(s))
                {
                    ///End Parenthesis
                    if (s == ")")
                    {
                        ///Deletes parenthesis, adds all other ops
                        OperatorToken latest = operatorStack.Pop();
                        while (latest.operation != "(")
                        {
                            outputStack.Add(latest);
                            latest = operatorStack.Pop();
                        }
                    }
                    ///Basic Operator
                    else
                    {
                        OperatorToken op = new OperatorToken(s);
                        while (operatorStack.Count != 0 && operatorStack.Peek().operation != "(" && OperatorToken.precedence[operatorStack.Peek().operation] >= OperatorToken.precedence[s])
                        {
                            outputStack.Add(operatorStack.Pop());
                        }
                        operatorStack.Push(op);
                    }
                }
                ///Not a basic operator; function or value
                else
                {
                    ///Separate Unary Operators
                    //char unary = ' ';                    
                    if (s.StartsWith('-'))
                    {
                        //unary = '~';
                        operatorStack.Push(new OperatorToken("~"));
                        s = s.Substring(1);
                    }
                    else if (s.StartsWith('+'))
                    {
                        //unary = '`';
                        operatorStack.Push(new OperatorToken("`"));
                        s = s.Substring(1);
                    }

                    ///Determine if function or value
                    if (Regex.IsMatch(s, @"(\w+)\((.+)\)\.?([x|y|z|w]{0,4})"))
                    {
                        ///Function
                        Match match = Regex.Match(s, @"(\w+)\((.+)\)\.?([x|y|z|w]{0,4})");
                        FunctionOperatorToken op = new FunctionOperatorToken(
                            match.Groups[1].Value,
                            match.Groups[2].Value,
                            match.Groups[3].Value
                        );
                        if (op.ParameterBody != null)
                        {
                            foreach (string p in splitParams(op.ParameterBody, ", "))
                            {
                                //TODO: Recursively generate node trees
                                op.children.Add(parseEquation(p));
                            }
                        }
                        ///Function is complete; treat as value
                        outputStack.Add(op);
                    }
                    else
                    {
                        ///Number
                        float parsedf;
                        int parsedi;
                        if (float.TryParse(s, out parsedf))
                        {
                            FloatValueToken f = new FloatValueToken(parsedf);
                            outputStack.Add(f);
                        }
                        else if (int.TryParse(s, out parsedi))
                        {
                            FloatValueToken f = new FloatValueToken((float)parsedi);
                            outputStack.Add(f);
                        }
                        ///Value
                        else if (Regex.IsMatch(s, @"([\w|\[|\]}]+)\.([x|y|z|w]{0,4})"))
                        {
                            ///Variable
                            Match match = Regex.Match(s, @"([\w|\[|\]}]+)\.([x|y|z|w]{0,4})");
                            VarValueToken var = new VarValueToken(match.Groups[1].Value, match.Groups[2].Value);
                            outputStack.Add(var);
                        }
                        else
                        {
                            ///UNKNOWN
                            BasicValueToken b = new BasicValueToken(s);
                            outputStack.Add(b);
                        }
                    }
                }
            }
            OperatorToken? remaining_op;
            while (operatorStack.TryPeek(out remaining_op))
            {
                outputStack.Add(operatorStack.Pop());
            }
            return eqToTree(outputStack);
        }
        private IToken eqToTree(List<IToken> equation)
        {
            if (equation.Count == 0)
            {
                return null;
            }
            if (equation.Count == 1)
            {
                return equation[0];
            }
            Queue<IToken> operands = new Queue<IToken>();
            foreach (IToken member in equation)
            {
                if (member is IValueToken)
                {
                    operands.Enqueue(member);
                }
                else
                {
                    OperatorToken op = member as OperatorToken;
                    if (op.Precedence() < 7)
                    {
                        //Binary
                        op.children.Add(operands.Dequeue());
                        op.children.Add(operands.Dequeue());
                        operands.Enqueue(op);
                    }
                    else if (op.Precedence() == 7)
                    {
                        //Unary
                        //This is expensive and bad but its the only way to pop the most recent item in the Queue (i.e. Dequeueing from the wrong end)
                        List<IToken> tokens = operands.ToList();
                        op.children.Add(tokens.Last());
                        tokens.RemoveAt(tokens.Count - 1);
                        operands = new Queue<IToken>();
                        foreach (IToken token in tokens)
                        {
                            operands.Enqueue(token);
                        }
                        operands.Enqueue(op);
                    }
                    else
                    {
                        operands.Enqueue(member);
                    }
                }
            }
            return operands.First() as OperatorToken;
        }
        private static string[] splitParams(string ParamString, string splitBy = ", ")
        {
            string[] Params = ParamString.Split(splitBy);
            int paraCount = 0;
            List<string> output = new List<string>();
            string temp = "";
            foreach (string param in Params)
            {
                paraCount += param.Count(c => c == '(') - param.Count(c => c == ')');
                if (paraCount == 0)
                {
                    temp += param;
                    output.Add(temp);
                    temp = "";
                }
                else
                {
                    //Parantheses unbalanced
                    temp += param + splitBy;
                }
            }
            return output.ToArray();
        }
    }
}