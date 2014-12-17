using System;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    class HqlCategory
    {
        private HqlCategory() { }

        // TODO, replace this with Int32.TryAndParse(), how much faster/slower is it??
        static public bool IsInt(string s)
        {
            // Not going to trim this as it should have been trimmed already
            bool FoundNumber = false;
            for (int i = 0; i < s.Length; ++i)
            {
                char c = s[i];

                if (Char.IsDigit(c))
                    FoundNumber = true;
                else if (c == '-' && i == 0)
                    continue;
                else if (c == '-' && i != 0)
                    return false;
                else if (c == '.')
                    return false;
                else
                    return false; 
            }
            return FoundNumber;
        }

        // TODO, replace this with Decimal.TryAndParse(), how much faster/slower is it??
        static public bool IsFloat(string s)
        {
            // Not going to trim this as it should have been trimmed already
            bool FoundNumber = false;
            bool FoundDecimalPoint = false;
            for (int i = 0; i < s.Length; ++i)
            {
                char c = s[i];

                if (Char.IsDigit(c))
                    FoundNumber = true;
                else if (c == '-' && i == 0)
                    continue;
                else if (c == '-' && i != 0)
                    return false;
                else if (c == '.' && !FoundDecimalPoint)
                    FoundDecimalPoint = true;
                else
                    return false;
            }
            return FoundNumber;
        }

        static public string PrintDefaultDecimal(decimal d)
        {
            string s = d.ToString("0.000000").TrimEnd('0');
            if (s.EndsWith(".", StringComparison.CurrentCulture))
                return s.TrimEnd('.');
            return s;
        }
    }
}
