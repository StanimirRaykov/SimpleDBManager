using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Management_System.Utilities
{
    public static class Functions
    {
        public static List<string> StringSplit(string input, char separator)
        {
            var result = new List<string>();
            int start = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == separator)
                {
                    result.Add(input.Substring(start, i - start));
                    start = i + 1;
                }
            }

            if (start < input.Length)
            {
                result.Add((input.Substring(start)));
            }

            return result;
        }
    }
}
