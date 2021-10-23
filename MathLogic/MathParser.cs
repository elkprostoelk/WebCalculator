using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;

namespace IntegralCalculator
{
    public class MathParser
    {
        private const string NumberMarker = "#",
            OperatorMarker = "$", FunctionMarker = "@", Plus = OperatorMarker + "+",
            UnPlus = OperatorMarker + "un+", Minus = OperatorMarker + "-",
            UnMinus = OperatorMarker + "un-", Multiply = OperatorMarker + "*",
            Divide = OperatorMarker + "/", Degree = OperatorMarker + "^",
            LeftParent = OperatorMarker + "(", RightParent = OperatorMarker + ")",
            Sqrt = FunctionMarker + "sqrt", Sin = FunctionMarker + "sin",
            Cos = FunctionMarker + "cos", Tg = FunctionMarker + "tg",
            Ctg = FunctionMarker + "ctg", Sh = FunctionMarker + "sh",
            Ch = FunctionMarker + "ch", Th = FunctionMarker + "th",
            Log = FunctionMarker + "log", Ln = FunctionMarker + "ln",
            Exp = FunctionMarker + "exp", Abs = FunctionMarker + "abs",
            Arcsin = FunctionMarker + "arcsin", Arccos = FunctionMarker + "arccos",
            Arctg = FunctionMarker + "arctg";

        private readonly Dictionary<string, string> supportedOperators =
            new()
            {
                { "+", Plus },
                { "-", Minus },
                { "*", Multiply },
                { "/", Divide },
                { "^", Degree },
                { "(", LeftParent },
                { ")", RightParent }
            };
        private readonly Dictionary<string, string> supportedFunctions =
            new()
            {
                { "sqrt", Sqrt },
                { "√", Sqrt },
                { "sin", Sin },
                { "cos", Cos },
                { "tg", Tg },
                { "ctg", Ctg },
                { "sh", Sh },
                { "ch", Ch },
                { "th", Th },
                { "ln", Ln },
                { "log", Log },
                { "exp", Exp },
                { "abs", Abs },
                { "arcsin", Arcsin },
                { "arccos", Arccos },
                { "arctg", Arctg },
            };
        private readonly Dictionary<string, string> supportedConstants =
            new()
            {
                {"pi", NumberMarker +  Math.PI.ToString() },
                {"e", NumberMarker + Math.E.ToString() }
            };

        private readonly char _decimalSeparator;
        private bool _isRadians;

        public MathParser()
        {
            _decimalSeparator = char.Parse(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
        }
        public MathParser(char decimalSeparator)
        {
            _decimalSeparator = decimalSeparator;
        }
        public double Parse(string expression, bool isRadians = true)
        {
            _isRadians = isRadians;
            return Calculate(ConvertToRPN(FormatString(expression)));
        }
        private static string FormatString(string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                throw new ArgumentNullException("Выражение равно нулю или отсутствует");
            }
            var formattedString = new StringBuilder();
            int balanceOfParenth = 0;
            for (int i = 0; i < expression.Length; i++)
            {
                char ch = expression[i];
                if (ch == '(')
                {
                    balanceOfParenth++;
                }
                else if (ch == ')')
                {
                    balanceOfParenth--;
                }
                if (char.IsWhiteSpace(ch))
                {
                    continue;
                }
                else if (char.IsUpper(ch))
                {
                    formattedString.Append(char.ToLower(ch));
                }
                else
                {
                    formattedString.Append(ch);
                }
            }
            if (balanceOfParenth != 0)
            {
                throw new FormatException("Количества открывающих и закрывающих скобок различаются");
            }
            return formattedString.ToString();
        }
        private string ConvertToRPN(string expression)
        {
            int pos = 0;
            var outputString = new StringBuilder();
            var stack = new Stack<string>();
            while (pos < expression.Length)
            {
                string token = LexicalAnalysisInfixNotation(expression, ref pos);

                outputString = SyntaxAnalysisInfixNotation(token, outputString, stack);
            }            
            while (stack.Count > 0)
            {
                if (stack.Peek()[0] == OperatorMarker[0])
                {
                    outputString.Append(stack.Pop());
                }
                else
                {
                    throw new FormatException("Ошибка формата, присутствуют функции без скобок");
                }
            }
            return outputString.ToString();
        }
        private string LexicalAnalysisInfixNotation(string expression, ref int pos)
        {
            var token = new StringBuilder();
            token.Append(expression[pos]);
            if (supportedOperators.ContainsKey(token.ToString()))
            {
                bool isUnary = pos == 0 || expression[pos - 1] == '(';
                pos++;
                return token.ToString() switch
                {
                    "+" => isUnary ? UnPlus : Plus,
                    "-" => isUnary ? UnMinus : Minus,
                    _ => supportedOperators[token.ToString()],
                };
            }
            else if (char.IsLetter(token[0]) || supportedFunctions.ContainsKey(token.ToString())
                || supportedConstants.ContainsKey(token.ToString()))
            {
                while (++pos < expression.Length
                    && char.IsLetter(expression[pos]))
                {
                    token.Append(expression[pos]);
                }

                if (supportedFunctions.ContainsKey(token.ToString()))
                {
                    return supportedFunctions[token.ToString()];
                }
                else if (supportedConstants.ContainsKey(token.ToString()))
                {
                    return supportedConstants[token.ToString()];
                }
                else
                {
                    throw new ArgumentException("Неизвестный знак");
                }

            }
            else if (char.IsDigit(token[0]) || token[0] == _decimalSeparator)
            {
                if (char.IsDigit(token[0]))
                {
                    while (++pos < expression.Length && char.IsDigit(expression[pos]))
                    {
                        token.Append(expression[pos]);
                    }
                }
                else token.Clear();
                if (pos < expression.Length && expression[pos] == _decimalSeparator)
                {
                    token.Append(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                    while (++pos < expression.Length
                    && char.IsDigit(expression[pos]))
                    {
                        token.Append(expression[pos]);
                    }
                }
                if (pos + 1 < expression.Length && expression[pos] == 'e' && (char.IsDigit(expression[pos + 1])
                        || (pos + 2 < expression.Length && (expression[pos + 1] == '+'
                                || expression[pos + 1] == '-') && char.IsDigit(expression[pos + 2]))))
                {
                    token.Append(expression[pos++]);
                    if (expression[pos] == '+' || expression[pos] == '-')
                        token.Append(expression[pos++]);
                    while (pos < expression.Length
                        && char.IsDigit(expression[pos]))
                    {
                        token.Append(expression[pos++]);
                    }
                    return NumberMarker + Convert.ToDouble(token.ToString());
                }
                return NumberMarker + token.ToString();
            }
            else throw new ArgumentException("Unknown token in expression");
        }
        private static StringBuilder SyntaxAnalysisInfixNotation(string token, StringBuilder outputString, Stack<string> stack)
        {            
            if (token[0] == NumberMarker[0])
                outputString.Append(token);
            else if (token[0] == FunctionMarker[0])
                stack.Push(token);
            else if (token == LeftParent)
                stack.Push(token);
            else if (token == RightParent)
            {
                string elem;
                while ((elem = stack.Pop()) != LeftParent)
                {
                    outputString.Append(elem);
                }
                if (stack.Count > 0 &&
                    stack.Peek()[0] == FunctionMarker[0])
                {
                    outputString.Append(stack.Pop());
                }
            }
            else
            {
                while (stack.Count > 0 &&
                    Priority(token, stack.Peek()))
                {
                    outputString.Append(stack.Pop());
                }
                stack.Push(token);
            }
            return outputString;
        }
        private static bool Priority(string token, string p)
        {
            return IsRightAssociated(token) ?
                GetPriority(token) < GetPriority(p) :
                GetPriority(token) <= GetPriority(p);
        }
        private static bool IsRightAssociated(string token)
        {
            return token == Degree;
        }
        private static int GetPriority(string token)
        {
            return token switch
            {
                LeftParent => 0,
                Plus or Minus => 2,
                UnPlus or UnMinus => 6,
                Multiply or Divide => 4,
                Degree or Sqrt => 8,
                Sin or Cos or Tg or Ctg or Sh or Ch or Th or Log or Ln or Exp or Abs or Arcsin or Arccos or Arctg => 10,
                _ => throw new ArgumentException("Неизвестный оператор"),
            };
        }
        private double Calculate(string expression)
        {
            int pos = 0;
            var stack = new Stack<double>();
            while (pos < expression.Length)
            {
                string token = LexicalAnalysisRPN(expression, ref pos);

                stack = SyntaxAnalysisRPN(stack, token);
            }
            if (stack.Count > 1)
            {
                throw new ArgumentException("Избыточный операнд");
            }
            return stack.Pop();
        }
        private static string LexicalAnalysisRPN(string expression, ref int pos)
        {
            var token = new StringBuilder();
            token.Append(expression[pos++]);
            while (pos < expression.Length && expression[pos] != NumberMarker[0]
                && expression[pos] != OperatorMarker[0]
                && expression[pos] != FunctionMarker[0])
            {
                token.Append(expression[pos++]);
            }
            return token.ToString();
        }
        private Stack<double> SyntaxAnalysisRPN(Stack<double> stack, string token)
        {
            if (token[0] == NumberMarker[0])
            {
                stack.Push(double.Parse(token.Remove(0, 1)));
            }
            else if (NumberOfArguments(token) == 1)
            {
                double arg = stack.Pop();
                var rst = token switch
                {
                    UnPlus => arg,
                    UnMinus => -arg,
                    Sqrt => Math.Sqrt(arg),
                    Sin => ApplyTrigFunction(Math.Sin, arg),
                    Cos => ApplyTrigFunction(Math.Cos, arg),
                    Tg => ApplyTrigFunction(Math.Tan, arg),
                    Ctg => 1 / ApplyTrigFunction(Math.Tan, arg),
                    Sh => Math.Sinh(arg),
                    Ch => Math.Cosh(arg),
                    Th => Math.Tanh(arg),
                    Ln => Math.Log(arg),
                    Exp => Math.Exp(arg),
                    Abs => Math.Abs(arg),
                    Arcsin => Math.Asin(arg),
                    Arccos => Math.Acos(arg),
                    Arctg => Math.Atan(arg),
                    _ => throw new ArgumentException("Unknown operator"),
                };
                stack.Push(rst);
            }
            else
            {
                double arg2 = stack.Pop();
                double arg1 = stack.Pop();
                double rst;
                switch (token)
                {
                    case Plus:
                        rst = arg1 + arg2;
                        break;
                    case Minus:
                        rst = arg1 - arg2;
                        break;
                    case Multiply:
                        rst = arg1 * arg2;
                        break;
                    case Divide:
                        if (arg2 == 0)
                        {
                            throw new DivideByZeroException("Второй аргумент равен нулю");
                        }
                        rst = arg1 / arg2;
                        break;
                    case Degree:
                        rst = Math.Pow(arg1, arg2);
                        break;
                    case Log:
                        rst = Math.Log(arg2, arg1);
                        break;
                    default:
                        throw new ArgumentException("Неизвестный оператор");
                }
                stack.Push(rst);
            }
            return stack;
        }
        private double ApplyTrigFunction(Func<double, double> func, double arg)
        {
            if (!_isRadians)
            {
                arg = arg * Math.PI / 180;
            }
            return func(arg);
        }
        private static int NumberOfArguments(string token)
        {
            return token switch
            {
                UnPlus or UnMinus or Sqrt or Tg or Sh or Ch or Th or Ln or Ctg or Sin or Cos or Exp or Abs or Arcsin or Arccos or Arctg => 1,
                Plus or Minus or Multiply or Divide or Degree or Log => 2,
                _ => throw new ArgumentException("Неизвестный оператор"),
            };
        }
    }
}