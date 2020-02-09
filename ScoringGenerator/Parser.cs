using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoringGenerator
{
    public static class Parser
    {
        static Dictionary<char, int> precedence = new Dictionary<char, int> { { '|', 0 }, { '&', 1 }, { '!', 2 } };
        public static string ToPostfix(string expr)
        {
            return expr;
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
                else if (c == ' ')
                {
                    refs.Add(_ref);
                    finding = false;
                }
            }
            if (finding)
                refs.Add(_ref);
        }
    }
}
