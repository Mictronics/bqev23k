using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Linq;

namespace BQEV23K
{
    /// <summary>
    /// Class parsing and evaluating the scaling formulas defined in device register configuration.
    /// </summary>
    /// <remarks>Not fully tested. Might have problems with negative values.</remarks>
    public class FormulaEval
    {
        private string[] _operators = { "-", "+", "/", "*", "^" };
        private Func<double, double, double>[] _operations = {
        (a1, a2) => a1 - a2,
        (a1, a2) => a1 + a2,
        (a1, a2) => a1 / a2,
        (a1, a2) => a1 * a2,
        (a1, a2) => Math.Pow(a1, a2)
        };

        /// <summary>
        /// Validate formula string.
        /// </summary>
        /// <param name="s">Formula string to validate.</param>
        /// <returns>True when valid, otherwise false.</returns>
        private static bool ValidateFormat(string s)
        {
            if (s == null || s.Length == 0 || !s.Contains("x"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Evaluate scaling formula and register value.
        /// </summary>
        /// <param name="formula">Scaling formula.</param>
        /// <param name="value">Register value.</param>
        /// <returns>Evaluation result or 0.0 on error.</returns>
        public double Eval(string formula, string value)
        {
            if (ValidateFormat(formula))
                return 0.0;

            if (value != null && value.Length > 0)
            {
                if(formula.Length == 1)
                {
                    return double.Parse(value);
                }
                else
                    formula = formula.Replace("x", value);
            }
            else
                return 0.0;

            return calculate(formula);
        }

        /// <summary>
        /// Calculate scaling formula.
        /// </summary>
        /// <param name="formula">Formula string to calculate.</param>
        /// <returns>Result.</returns>
        private double calculate(string formula)
        {
            List<string> tokens = getTokens(formula);
            Stack<double> operandStack = new Stack<double>();
            Stack<string> operatorStack = new Stack<string>();
            int tokenIndex = 0;

            while (tokenIndex < tokens.Count)
            {
                string token = tokens[tokenIndex];
                if (token == "(")
                {
                    string subExpr = getSubExpression(tokens, ref tokenIndex);
                    operandStack.Push(calculate(subExpr));
                    continue;
                }
                if (token == ")")
                {
                    throw new ArgumentException("Mis-matched parentheses in expression");
                }
                //If this is an operator  
                if (Array.IndexOf(_operators, token) >= 0)
                {
                    while (operatorStack.Count > 0 && Array.IndexOf(_operators, token) < Array.IndexOf(_operators, operatorStack.Peek()))
                    {
                        string op = operatorStack.Pop();
                        double arg2 = operandStack.Pop();
                        double arg1 = operandStack.Pop();
                        operandStack.Push(_operations[Array.IndexOf(_operators, op)](arg1, arg2));
                    }
                    operatorStack.Push(token);
                }
                else
                {
                    operandStack.Push(double.Parse(token));
                }
                tokenIndex += 1;
            }

            while (operatorStack.Count > 0)
            {
                string op = operatorStack.Pop();
                double arg2 = operandStack.Pop();
                double arg1 = operandStack.Pop();
                operandStack.Push(_operations[Array.IndexOf(_operators, op)](arg1, arg2));
            }
            return operandStack.Pop();
        }

        /// <summary>
        /// Get sub calculations in parentheses.
        /// </summary>
        /// <param name="tokens">List of parentheses token.</param>
        /// <param name="index">Token index.</param>
        /// <returns>Sub calculation string.</returns>
        private string getSubExpression(List<string> tokens, ref int index)
        {
            StringBuilder subExpr = new StringBuilder();
            int parenlevels = 1;
            index += 1;
            while (index < tokens.Count && parenlevels > 0)
            {
                string token = tokens[index];
                if (tokens[index] == "(")
                {
                    parenlevels += 1;
                }

                if (tokens[index] == ")")
                {
                    parenlevels -= 1;
                }

                if (parenlevels > 0)
                {
                    subExpr.Append(token);
                }

                index += 1;
            }

            if ((parenlevels > 0))
            {
                throw new ArgumentException("Mis-matched parentheses in expression");
            }
            return subExpr.ToString();
        }

        /// <summary>
        /// Get arithmetic token.
        /// </summary>
        /// <param name="expression">Scaling formula.</param>
        /// <returns>Arithmetic token list.</returns>
        private List<string> getTokens(string expression)
        {
            string operators = "()^*/+-";
            List<string> tokens = new List<string>();
            StringBuilder sb = new StringBuilder();

            foreach (char c in expression.Replace(" ", string.Empty))
            {
                if (operators.IndexOf(c) >= 0)
                {
                    if ((sb.Length > 0))
                    {
                        tokens.Add(sb.ToString());
                        sb.Length = 0;
                    }
                    tokens.Add(c.ToString());
                }
                else
                {
                    sb.Append(c);
                }
            }

            if ((sb.Length > 0))
            {
                tokens.Add(sb.ToString());
            }
            return tokens;
        }

        /// <summary>
        /// Evaluate a C# code string.
        /// </summary>
        /// <param name="code">C# code string to evaluate.</param>
        /// <param name="outType">Return type.</param>
        /// <param name="includeNamespaces">Namespaces to include.</param>
        /// <param name="includeAssemblies">Assemblies to include.</param>
        /// <returns>Evaluation result.</returns>
        public static object EvalCSharp(string code, Type outType = null, string[] includeNamespaces = null, string[] includeAssemblies = null)
        {
            StringBuilder namespaces = null;
            object methodResult = null;
            using (CodeDomProvider codeProvider = CodeDomProvider.CreateProvider("CSharp"))
            {
                CompilerParameters compileParams = new CompilerParameters();
                compileParams.CompilerOptions = "/t:library";
                compileParams.GenerateInMemory = true;
                if (includeAssemblies != null && includeAssemblies.Any())
                {
                    foreach (string _assembly in includeAssemblies)
                    {
                        compileParams.ReferencedAssemblies.Add(_assembly);
                    }
                }

                if (includeNamespaces != null && includeNamespaces.Any())
                {
                    foreach (string _namespace in includeNamespaces)
                    {
                        namespaces = new StringBuilder();
                        namespaces.Append(string.Format("using {0};\n", _namespace));
                    }
                }
                code = string.Format(
                    @"{1}
                using System;
                namespace CSharpCode{{
                    public class Parser{{
                        public {2} Eval(){{
                            {3} {0};
                        }}
                    }}
                }}",
                    code,
                    namespaces != null ? namespaces.ToString() : null,
                    outType != null ? outType.FullName : "void",
                    outType != null ? "return" : string.Empty
                    );
                CompilerResults compileResult = codeProvider.CompileAssemblyFromSource(compileParams, code);

                if (compileResult.Errors.Count > 0)
                {
                    throw new Exception(compileResult.Errors[0].ErrorText);
                }
                System.Reflection.Assembly assembly = compileResult.CompiledAssembly;
                object classInstance = assembly.CreateInstance("CSharpCode.Parser");
                Type type = classInstance.GetType();
                MethodInfo methodInfo = type.GetMethod("Eval");
                methodResult = methodInfo.Invoke(classInstance, null);
            }
            return methodResult;
        }
    }
}
