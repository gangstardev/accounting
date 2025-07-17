using System;
using System.Globalization;
using System.Text;

namespace AccountingApp
{
    public static class PersianTextUtils
    {
        // تبدیل متن فارسی به صورت چسبیده و راست‌به‌چپ (ساده و کاربردی برای فاکتور)
        public static string FixPersianText(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            // اعداد را جدا جدا نکن و متن را برعکس کن (برای PdfSharp)
            var sb = new StringBuilder();
            var lines = input.Split('\n');
            foreach (var line in lines)
            {
                // اعداد و کاراکترهای لاتین را جدا جدا برعکس نکن
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