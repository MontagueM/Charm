using Field.General;
using Newtonsoft.Json.Linq;
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
            return $"NODE_{line_id}_{var_index}";
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
        public HLSLParser(string line_id, string hash)
        {
            this.line_id = line_id;
            //this.hash = hash;
        }
        public string parseEquationFull(string equation, string variable, string dimensions)
        {
            IToken tree = parseEquation(equation);
            //string inputVar = $"{variable}.{dimensions}";
            StringBuilder nodeBody = new StringBuilder();
            StringBuilder varAssignments = new StringBuilder();
            for (int i = 0; i < dimensions.Length; i++)
            {
                //Process tree to only have one dimension
                IToken dimTree = getDimension(tree.Clone(), i, dimensions);
                Tuple<string, string> traversal = traverse(dimTree);
                nodeBody.AppendLine(traversal.Item1);
                varAssignments.AppendLine($"variable_dict['{variable}.{dimensions[i]}'] = {traversal.Item2}");
            }
            //Variable assignments come afterward to avoid interference
            nodeBody.AppendLine(varAssignments.ToString());
            return nodeBody.ToString();
        }
        private IToken getDimension(IToken tree, int i, string dims)
        {
            string[] dimMap = new string[] { "x", "y", "z", "w" };
            if (tree == null)
            {
                return null;
            }
            else if (tree is OperatorToken)
            {
                ///Operator or Function
                //Function
                if (tree is FunctionOperatorToken)
                {
                    FunctionOperatorToken t = (FunctionOperatorToken)tree;
                    if (t.FunctionName.StartsWith("float") || t.FunctionName.StartsWith("combine"))
                    {
                        return t.children[i % t.children.Count];
                    }
                    bool hasDims = t.Dimensions != null && t.Dimensions?.Length != 0;
                    ///With how this is handled, each function gets duplicated for each dimension
                    ///This is good for single-dimension functions but will mean that things like textures will be copied unnessasarily                    
                    if (!hasDims)
                    {
                        //t.Dimensions = dimMap[i];
                    }
                    else
                    {
                        t.Dimensions = t.Dimensions?[i % t.Dimensions.Length].ToString();
                    }
                    //TODO: Handle function parameters
                    //Example Cases:
                    ///Scalar function used in a single dimension
                    //r0.x = sqrt(r0.x)
                    ///Scalar function used across dimensions
                    //r0.xy = abs(v3.zy);
                    ///Multi-dimension function used to implicitly assign multiple different dimensions
                    //o0.xyz = sample(0, r0.xy)
                    ///Multi-dimension function explicitly assigns dimensions
                    //v3.y = cross(r0.xy, cb0[3].yz).x

                    for (int idx = 0; idx < t.children.Count; idx++)
                    {
                        IToken child = t.children[idx];
                        //If parameter is 1-dimensional, split dimension
                        if (isOperation1D(t.FunctionName, idx))
                        {
                            if (child is FunctionOperatorToken)
                            {
                                FunctionOperatorToken f = (FunctionOperatorToken)child;
                                if (f.FunctionName.StartsWith("combine"))
                                {
                                    t.children[idx] = f.children[i % f.children.Count];
                                }
                                else
                                {
                                    t.children[idx] = getDimension(f, i, dims);
                                }
                            }
                            else {
                                t.children[idx] = getDimension(child, i, dims);
                            }                        
                        }
                        //Else parameter should match dimension exactly
                        else
                        {
                            t.children[idx] = getDimension(child, i, dims);
                        }
                    }
                    return t;
                }
                else
                {
                    OperatorToken t = (OperatorToken)tree;
                    for (int idx = 0; idx < t.children.Count(); idx++)
                    {
                        IToken child = t.children[idx];
                        if (isOperation1D(t.operation, idx))
                        {
                            if (child is FunctionOperatorToken)
                            {
                                FunctionOperatorToken f = (FunctionOperatorToken)child;
                                if (f.FunctionName.StartsWith("combine"))
                                {
                                    t.children[idx] = f.children[i % f.children.Count];
                                }
                                else
                                {
                                    t.children[idx] = getDimension(f, i, dims);
                                }
                            }
                            else
                            {
                                t.children[idx] = getDimension(child, i, dims);
                            }
                        }
                        //Else parameter should match dimension exactly
                        else
                        {
                            t.children[idx] = getDimension(child, i, dims);
                        }
                    }
                    return t;
                }
            }
            else
            {
                ///Value
                if (tree is VarValueToken)
                {                    
                    VarValueToken t = (VarValueToken)tree;
                    if (i < t.Dimensions?.Length)
                    {
                        t.Dimensions = t.Dimensions?[i].ToString();
                    }
                    else
                    {
                        t.Dimensions = t.Dimensions?[i % t.Dimensions.Length].ToString();
                    }                    
                    return t;
                }
                else
                {
                    //Numbers can go right through
                    return tree;
                }
            }
        }

        private bool isOperation1D(string operation, int param) {
            if (operation.StartsWith("ddx") || operation.StartsWith("ddy"))
            {
                return param switch
                {
                    -1 => true,
                    0 => false
                };
            }
            switch (operation)
            {
                case "sample":
                    return param switch
                    {
                        -1 => false,
                        0 => true, //Should never actually be anything other than a float anyway
                        1 => false
                    };
                case "dot":
                    return param switch
                    {
                        -1 => true,
                        0 => false,
                        1 => false
                    };
                case "cross":
                    return param switch
                    {
                        -1 => false,
                        0 => false,
                        1 => false
                    };
                default:
                    return true;
            }
        }
        private Tuple<string, string> traverse(IToken tree)
        {
            Dictionary<string, int> dimIdx = new Dictionary<string, int>() {
                { "x", 0 },
                { "y", 1 },
                { "z", 2 },
                { "w", 3 },
            };
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
                Tuple<string, string> operatorNodeS;
                if (operatorToken is FunctionOperatorToken)
                {
                    FunctionOperatorToken functionOperatorToken = (FunctionOperatorToken)operatorToken;
                    if (functionOperatorToken.FunctionName == "sample") {
                        operatorNodeS = operatorNode(functionOperatorToken.FunctionName, paramConnectors.ToArray(), 
                            functionOperatorToken.Dimensions?.Length == 0 ? -1 : dimIdx[functionOperatorToken.Dimensions.Substring(0, 1)]);
                    }
                    else
                    {
                        operatorNodeS = operatorNode(functionOperatorToken.FunctionName, paramConnectors.ToArray());
                    }
                }
                else
                {
                    operatorNodeS = operatorNode(operatorToken.operation, paramConnectors.ToArray());
                }
                
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
        private Tuple<string, string> operatorNode(string operation, string[] paramConnectors, int dim = -1)
        {
            StringBuilder outputScript = new StringBuilder();
            string outputConnector = "";
            string name = createIdentifier();
            switch (operation)
            {
                ///https://github.com/bo3b/3Dmigoto/blob/88633bcef119bde3a4c23c31fadb6fa05dbb66ea/HLSLDecompiler/DecompileHLSL.cpp#L2769
                ///cmp returns -1 if the boolean inside is true, otherwise 0
                case "cmp":
                case "~": //Unary -
                    outputScript.AppendLine($@"{name} = matnodes.new(""ShaderNodeMath"")");
                    outputScript.AppendLine($@"{name}.operation = 'MULTIPLY'");
                    outputScript.AppendLine($@"{name}.inputs[0].default_value = -1.0");
                    outputScript.AppendLine($@"link({paramConnectors[0]}, {name}.inputs[1])");
                    outputConnector = $@"{name}.outputs[0]";
                    break;                
                case "`": //Unary +
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
                case ">":
                case ">=":
                    outputScript.AppendLine($@"{name} = matnodes.new(""ShaderNodeMath"")");
                    outputScript.AppendLine($@"{name}.operation = 'GREATER_THAN'");
                    outputScript.AppendLine($@"link({paramConnectors[0]}, {name}.inputs[0])");
                    outputScript.AppendLine($@"link({paramConnectors[1]}, {name}.inputs[1])");
                    outputConnector = $@"{name}.outputs[0]";
                    break;
                case "<":
                case "<=":
                    outputScript.AppendLine($@"{name} = matnodes.new(""ShaderNodeMath"")");
                    outputScript.AppendLine($@"{name}.operation = 'LESS_THAN'");
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
                    outputScript.AppendLine($@"{name}.blend_type = 'VALUE'");
                    outputScript.AppendLine($@"link({paramConnectors[0]}, {name}.inputs[0])");
                    outputScript.AppendLine($@"link({paramConnectors[1]}, {name}.inputs[1])");
                    outputScript.AppendLine($@"link({paramConnectors[2]}, {name}.inputs[2])");
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
                    outputScript.AppendLine($@"{name}.operation = 'MAXIMUM'");
                    outputScript.AppendLine($@"link({paramConnectors[0]}, {name}.inputs[0])");
                    outputScript.AppendLine($@"link({paramConnectors[1]}, {name}.inputs[1])");
                    outputConnector = $@"{name}.outputs[0]";
                    break;
                case "min":
                    outputScript.AppendLine($@"{name} = matnodes.new(""ShaderNodeMath"")");
                    outputScript.AppendLine($@"{name}.operation = 'MINIMUM'");
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
                case "combine2":
                    outputScript.AppendLine($@"{name} = matnodes.new(""ShaderNodeCombineXYZ"")");
                    outputScript.AppendLine($@"link({paramConnectors[0]}, {name}.inputs[0])");
                    outputScript.AppendLine($@"link({paramConnectors[1]}, {name}.inputs[1])");
                    outputConnector = $@"{name}.outputs[0]";
                    break;
                case "combine3":
                    outputScript.AppendLine($@"{name} = matnodes.new(""ShaderNodeCombineXYZ"")");
                    outputScript.AppendLine($@"link({paramConnectors[0]}, {name}.inputs[0])");
                    outputScript.AppendLine($@"link({paramConnectors[1]}, {name}.inputs[1])");
                    outputScript.AppendLine($@"link({paramConnectors[2]}, {name}.inputs[2])");
                    outputConnector = $@"{name}.outputs[0]";
                    break;
                //case "combine4":
                case "dot":
                    outputScript.AppendLine($@"{name} = matnodes.new(""ShaderNodeVectorMath"")");
                    outputScript.AppendLine($@"{name}.operation = 'DOT_PRODUCT'");
                    outputScript.AppendLine($@"link({paramConnectors[0]}, {name}.inputs[0])");
                    outputScript.AppendLine($@"link({paramConnectors[1]}, {name}.inputs[1])");
                    outputConnector = $@"{name}.outputs[0]";
                    break;
                case "sample":
                    outputScript.AppendLine($@"{name} = matnodes.new(""ShaderNodeTexImage"")");
                    outputScript.AppendLine($"{name}.label = get_tex_name({paramConnectors[0]}.default_value)");
                    outputScript.AppendLine($"{name}.image = get_texture({paramConnectors[0]}.default_value)");               
                    outputScript.AppendLine($@"link({paramConnectors[1]}, {name}.inputs[0])");
                    if (dim > 2) // 3
                    {
                        outputConnector = $@"{name}.outputs[1]";
                    }
                    else if (dim > -1) { //0 - 2
                        outputScript.AppendLine($@"{name}_split = matnodes.new(""ShaderNodeSeparateColor"")");
                        outputScript.AppendLine($@"link({name}.outputs[0], {name}_split.inputs[0])");
                        outputConnector = $@"{name}_split.outputs[{dim}]";
                    }                                        
                    break;
                case "rsqrt":
                    outputScript.AppendLine($@"{name}_root = matnodes.new(""ShaderNodeMath"")");
                    outputScript.AppendLine($@"{name}_root.operation = 'SQRT'");
                    outputScript.AppendLine($@"link({paramConnectors[0]}, {name}_root.inputs[0])");

                    outputScript.AppendLine($@"{name} = matnodes.new(""ShaderNodeMath"")");
                    outputScript.AppendLine($@"{name}.operation = 'DIVIDE'");
                    outputScript.AppendLine($@"{name}.inputs[0].default_value = 1.0");
                    outputScript.AppendLine($@"link({name}_root.outputs[0], {name}.inputs[1])");

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

            //Check for inline condition
            if (Regex.IsMatch(inputExp, "(\\S+) \\? (\\S+) \\: (\\S+)"))
            {
                Match match = Regex.Match(inputExp, "(\\S+) \\? (\\S+) \\: (\\S+)");
                FunctionOperatorToken op = new FunctionOperatorToken("mix");
                op.children.Add(parseEquation(match.Groups[1].Value));
                op.children.Add(parseEquation(match.Groups[2].Value));
                op.children.Add(parseEquation(match.Groups[3].Value));
                outputStack.Add(op);
                return eqToTree(outputStack);
            }

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
                            foreach (string p in splitParams(op.ParameterBody, ","))
                            {
                                //TODO: Recursively generate node trees
                                op.children.Add(parseEquation(p.Trim()));
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
                            Match match = Regex.Match(s, @"([a-zA-Z][\w|\[|\]}]+)\.([x|y|z|w]{0,4})");
                            VarValueToken var = new VarValueToken(match.Groups[1].Value, match.Groups[2].Value);
                            if (var.Dimensions?.Length > 1)
                            {
                                FunctionOperatorToken combine = new FunctionOperatorToken($"combine{var.Dimensions.Length}",dimensions:var.Dimensions);
                                foreach (char p in var.Dimensions.ToCharArray())
                                {
                                    combine.children.Add(new VarValueToken(var.Value, p.ToString()));
                                }
                                outputStack.Add(combine);
                            }
                            else
                            {
                                outputStack.Add(var);
                            }                            
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
