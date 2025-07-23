using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace AccountingApp.Utilities
{
    public static class ActiveXThreadingFix
    {
        [DllImport("ole32.dll")]
        private static extern int CoInitializeEx(IntPtr pvReserved, uint dwCoInit);

        [DllImport("ole32.dll")]
        private static extern void CoUninitialize();

        private const uint COINIT_APARTMENTTHREADED = 0x2;
        private const uint COINIT_MULTITHREADED = 0x0;

        public static void InitializeComForActiveX()
        {
            try
            {
                // تنظیم COM برای thread فعلی
                CoInitializeEx(IntPtr.Zero, COINIT_APARTMENTTHREADED);
            }
            catch (Exception ex)
            {
                // اگر خطا رخ داد، سعی می‌کنیم با تنظیمات مختلف
                try
                {
                    CoInitializeEx(IntPtr.Zero, COINIT_MULTITHREADED);
                }
                catch
                {
                    // نادیده گرفتن خطا در صورت عدم موفقیت
                }
            }
        }

        public static void CleanupCom()
        {
            try
            {
                CoUninitialize();
            }
            catch
            {
                // نادیده گرفتن خطا
            }
        }

        public static void RunInStaThread(Action action)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    InitializeComForActiveX();
                    action();
                }
                catch (Exception ex)
                {
                    // نمایش خطا در thread اصلی
                    Application.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show($"خطا در اجرای عملیات: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
                finally
                {
                    CleanupCom();
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join(10000); // انتظار حداکثر 10 ثانیه
        }

        public static T RunInStaThread<T>(Func<T> func)
        {
            T result = default(T);
            var thread = new Thread(() =>
            {
                try
                {
                    InitializeComForActiveX();
                    result = func();
                }
                catch (Exception ex)
                {
                    // نمایش خطا در thread اصلی
                    Application.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show($"خطا در اجرای عملیات: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
                finally
                {
                    CleanupCom();
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join(10000); // انتظار حداکثر 10 ثانیه
            return result;
        }
    }
} 