using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Field.Textures
{
    internal class Tokens
    {
        public interface IToken
        {
            
        }
        public class OperatorToken : IToken
        {
            public static Dictionary<string, int> precedence = new Dictionary<string, int>()
            {
                {"(", int.MaxValue},
                {")", int.MaxValue},
                {"function", int.MaxValue-1},
                {"`", 7 }, //Unary +
                {"~", 7 }, //Unary -
                {"*", 6},
                {"/", 6},
                {"%", 6},
                {"+", 5},
                {"-", 5},
                {">", 3},
                {"<", 3},
                {">=", 3},
                {"<=", 3},
            };
            public string operation;
            public List<IToken> children = new List<IToken>();
            public OperatorToken(string operation, List<IToken> children)
            {
                this.operation = operation;
                this.children = children;
            }
            public OperatorToken(string operation)
            {
                this.operation = operation;
            }
            public bool isEndingOperator()
            {
                foreach (IToken token in children)
                {
                    if (token.GetType() == typeof(OperatorToken))
                    {
                        return false;
                    }
                }
                //All tokens must be values
                return true;
            }
            public int Precedence()
            {
                return precedence[operation];
            }
            public string ToString()
            {
                return $"OperatorToken({operation})";
            }
        }
        public class FunctionOperatorToken : OperatorToken, IValueToken
        {
            public string FunctionName;
            public string? Dimensions;
            public string? ParameterBody;
            public FunctionOperatorToken(string operation, string parameterbody = null, string dimensions = null) : base("function")
            {
                FunctionName = operation;
                ParameterBody = parameterbody;
                Dimensions = dimensions;
            }
            public FunctionOperatorToken(string operation, List<IToken> children, string parameterbody = null, string dimensions = null) : base("function", children)
            {
                FunctionName = operation;
                ParameterBody = parameterbody;
                Dimensions = dimensions;
            }
            public string ToString()
            {
                return $"{operation}_{FunctionName}";
            }
        }
        public interface IValueToken : IToken
        {
            public string ToString();
        }
        public class BasicValueToken : IValueToken
        {
            public string Value;
            public BasicValueToken(string value)
            {
                Value = value;
            }

            public override string ToString()
            {
                return Value;
            }
        }
        public class FloatValueToken : IValueToken
        {
            public float Value;
            public FloatValueToken(float value)
            {
                Value = value;
            }

            public override string ToString()
            {
                return Value.ToString(CultureInfo.InvariantCulture);
            }
        }
        public class VarValueToken : IValueToken
        {
            public string Value;
            public string? Dimensions;
            public VarValueToken(string value, string dimensions = null)
            {
                Value = value;
                Dimensions = dimensions;
            }

            public override string ToString()
            {
                return $"{Value}.{Dimensions}";
            }
        }
    }
}
