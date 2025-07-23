using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AccountingApp.Utilities
{
    public static class ThreadSafePdfOpener
    {
        public static void OpenPdfSafely(string pdfPath)
        {
            if (!File.Exists(pdfPath))
            {
                MessageBox.Show("فایل PDF یافت نشد.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // روش 1: استفاده از STA thread برای جلوگیری از مشکلات ActiveX
                ActiveXThreadingFix.RunInStaThread(() => OpenPdfWithProcess(pdfPath));
            }
            catch (Exception ex)
            {
                // روش 2: استفاده از روش جایگزین
                try
                {
                    ActiveXThreadingFix.RunInStaThread(() => OpenPdfAlternative(pdfPath));
                }
                catch (Exception ex2)
                {
                    // روش 3: استفاده از برنامه پیش‌فرض
                    try
                    {
                        OpenPdfWithDefaultApp(pdfPath);
                    }
                    catch (Exception ex3)
                    {
                        MessageBox.Show($"خطا در باز کردن PDF:\n{ex.Message}\n\nروش‌های جایگزین نیز ناموفق بودند:\n{ex2.Message}\n{ex3.Message}", 
                            "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private static void OpenPdfWithProcess(string pdfPath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = pdfPath,
                UseShellExecute = true,
                Verb = "open"
            };

            // تنظیمات اضافی برای جلوگیری از مشکلات ActiveX
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.CreateNoWindow = false;

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                process.WaitForInputIdle(5000); // انتظار حداکثر 5 ثانیه
            }
        }

        private static void OpenPdfAlternative(string pdfPath)
        {
            // روش جایگزین: استفاده از دستورات سیستمی
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c start \"\" \"{pdfPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                process.WaitForExit(5000);
            }
        }

        public static async Task OpenPdfAsync(string pdfPath)
        {
            await Task.Run(() =>
            {
                try
                {
                    OpenPdfSafely(pdfPath);
                }
                catch (Exception ex)
                {
                    // نمایش خطا در thread اصلی
                    Application.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show($"خطا در باز کردن PDF: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
            });
        }

        public static void OpenPdfWithDefaultApp(string pdfPath)
        {
            try
            {
                // یافتن برنامه پیش‌فرض برای PDF
                var defaultApp = GetDefaultPdfViewer();
                if (!string.IsNullOrEmpty(defaultApp))
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = defaultApp,
                        Arguments = $"\"{pdfPath}\"",
                        UseShellExecute = false
                    };

                    using var process = Process.Start(startInfo);
                }
                else
                {
                    // استفاده از روش پیش‌فرض
                    OpenPdfSafely(pdfPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در باز کردن PDF: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string GetDefaultPdfViewer()
        {
            try
            {
                // بررسی برنامه‌های رایج PDF
                var commonViewers = new[]
                {
                    @"C:\Program Files\Adobe\Acrobat DC\Acrobat\Acrobat.exe",
                    @"C:\Program Files (x86)\Adobe\Acrobat Reader DC\Reader\AcroRd32.exe",
                    @"C:\Program Files\Adobe\Acrobat Reader DC\Reader\AcroRd32.exe",
                    @"C:\Program Files (x86)\Adobe\Reader\AcroRd32.exe",
                    @"C:\Program Files\Adobe\Reader\AcroRd32.exe",
                    @"C:\Program Files\SumatraPDF\SumatraPDF.exe",
                    @"C:\Program Files (x86)\SumatraPDF\SumatraPDF.exe"
                };

                foreach (var viewer in commonViewers)
                {
                    if (File.Exists(viewer))
                    {
                        return viewer;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
} 