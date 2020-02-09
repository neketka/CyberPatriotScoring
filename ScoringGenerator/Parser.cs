using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoringGenerator
{
    public class Tokenizer
    {
        private string[] tokens;
        public Tokenizer(string expr, params char[] tokenTypes)
        {
            List<string> tokens = new List<string>();
            string num = "";
            bool parsingNum = false;
            foreach (char c in expr)
            {
                if (char.IsDigit(c))
                {
                    num += c;
                    parsingNum = true;
                }
                else if (parsingNum)
                {
                    parsingNum = false;
                    tokens.Add(num);
                    num = "";
                }

                if (tokenTypes.Contains(c))
                {
                    tokens.Add(c.ToString());
                }
            }
            if (parsingNum)
                tokens.Add(num);
            this.tokens = tokens.ToArray();
        }

        public string[] GetTokens()
        {
            return tokens;
        }
    }
    public static class Parser
    {
        static Dictionary<char, int> precedence = new Dictionary<char, int> { { '#', -1 }, { '|', 0 }, { '&', 1 }, { '!', 2 } };
        public static string ToPostfix(string expr)
        {
            Stack<string> operationStack = new Stack<string>();
            string postfix = "";

            operationStack.Push("#");

            Tokenizer t = new Tokenizer(expr, '|', '&', '!', '(', ')');

            foreach (string token in t.GetTokens())
            {
                if (char.IsDigit(token[0]))
                {
                    postfix += token + " ";
                }
                else if (token == "(")
                {
                    operationStack.Push("(");
                }
                else if (token == ")")
                {
                    while (operationStack.Peek() != "(")
                    {
                        postfix += operationStack.Pop() + " ";
                    }
                    operationStack.Pop();
                }
                else if (token == "!")
                {
                    operationStack.Push("!");
                }
                else if (operationStack.Peek() == "(" || precedence[token[0]] >= precedence[operationStack.Peek()[0]])
                {
                    operationStack.Push(token);
                }
                else if (precedence[token[0]] < precedence[operationStack.Peek()[0]])
                {
                    while (operationStack.Peek() != "(" && operationStack.Peek() != "#" && precedence[operationStack.Peek()[0]] > precedence[token[0]])
                    {
                        postfix += operationStack.Pop() + " ";
                    }
                    operationStack.Push(token);
                }
            }
            while(operationStack.Peek() != "#")
            {
                postfix += operationStack.Pop() + " ";
            }

            return postfix;
        }

        public static bool EvalPostfix(string expr, params bool[] bools)
        {
            string[] dat = expr.Trim().Split(' ');
            Stack<bool> nums = new Stack<bool>();
            foreach (string s in dat)
            {
                if (int.TryParse(s, out int res))
                {
                    nums.Push(bools[res]);
                }
                else if (s == "&")
                {
                    bool last = nums.Pop();
                    bool first = nums.Pop();
                    nums.Push(first && last);
                }
                else if (s == "|")
                {
                    bool last = nums.Pop();
                    bool first = nums.Pop();
                    nums.Push(first || last);
                }
                else if (s == "!")
                {
                    bool last = nums.Pop();
                    nums.Push(!last);
                }
            }
            return nums.Peek();
        }

        public static void GetRefs(List<string> refs, string param, char refChar)
        {
            string _ref = "";
            bool finding = false;
            foreach (char c in param)
            {
                if (c == refChar)
                {
                    if (finding)
                        refs.Add(_ref);
                    else finding = true;
                }
                else if (c == ' ' && finding)
                {
                    refs.Add(_ref);
                    finding = false;
                }
                else if (finding)
                {
                    _ref += c;
                }
            }
            if (finding)
                refs.Add(_ref);
        }
    }
}
