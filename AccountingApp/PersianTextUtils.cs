using System;
using System.Globalization;
using System.Text;

namespace AccountingApp
{
    public static class PersianTextUtils
    {
        public static string FixPersianText(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var sb = new StringBuilder();
            var lines = input.Split('\n');
            foreach (var line in lines)
            {
                var temp = new StringBuilder();
                for (int i = line.Length - 1; i >= 0; i--)
                {
                    temp.Append(line[i]);
                }
                sb.AppendLine(temp.ToString());
            }
            return sb.ToString().TrimEnd();
        }
    }
} 