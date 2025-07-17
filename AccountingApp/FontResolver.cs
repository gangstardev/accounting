using System;
using System.IO;
using PdfSharp.Fonts;

namespace AccountingApp
{
    public class CustomFontResolver : IFontResolver
    {
        public byte[] GetFont(string faceName)
        {
            if (faceName == "Vazir#")
            {
                string fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "Vazir.ttf");
                if (File.Exists(fontPath))
                {
                    return File.ReadAllBytes(fontPath);
                }
            }
            throw new NotImplementedException($"Font {faceName} not found.");
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (familyName.Equals("Vazir", StringComparison.OrdinalIgnoreCase))
            {
                return new FontResolverInfo("Vazir#");
            }
            
            // رفع مشکل: استفاده از فونت پیش‌فرض سیستم
            try
            {
                return PlatformFontResolver.ResolveTypeface("Arial", isBold, isItalic);
            }
            catch
            {
                // در صورت عدم دسترسی به PlatformFontResolver
                return new FontResolverInfo("Arial");
            }
        }
    }
}