using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json;
using System.Diagnostics;
using System.Reflection;

namespace AccountingApp.Services
{
    public class UpdateService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _currentVersion;
        private readonly string _updateUrl = "https://api.github.com/repos/gangstardev/accounting/releases/latest";
        private readonly string _downloadUrl = "https://github.com/gangstardev/accounting/releases/latest/download/AccountingApp.zip";

        public UpdateService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "AccountingApp-UpdateChecker");
            _currentVersion = GetCurrentVersion();
        }

        private string GetCurrentVersion()
        {
            try
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                return version?.ToString() ?? "1.0.0.0";
            }
            catch
            {
                return "1.0.0.0";
            }
        }

        public async Task CheckForUpdatesAsync()
        {
            try
            {
                var latestVersion = await GetLatestVersionAsync();
                if (IsNewVersionAvailable(latestVersion))
                {
                    var result = MessageBox.Show(
                        $"نسخه جدید {latestVersion} موجود است!\n\n" +
                        $"نسخه فعلی: {_currentVersion}\n" +
                        $"نسخه جدید: {latestVersion}\n\n" +
                        "آیا می‌خواهید آپدیت را دانلود کنید؟",
                        "آپدیت موجود",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information
                    );

                    if (result == DialogResult.Yes)
                    {
                        await DownloadAndInstallUpdateAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                // در صورت خطا، فقط لاگ می‌کنیم و کاربر را آزار نمی‌دهیم
                Console.WriteLine($"خطا در بررسی آپدیت: {ex.Message}");
            }
        }

        private async Task<string> GetLatestVersionAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync(_updateUrl);
                var releaseInfo = JsonSerializer.Deserialize<GitHubRelease>(response);
                return releaseInfo?.tag_name?.TrimStart('v') ?? _currentVersion;
            }
            catch
            {
                return _currentVersion;
            }
        }

        private bool IsNewVersionAvailable(string latestVersion)
        {
            try
            {
                var current = Version.Parse(_currentVersion);
                var latest = Version.Parse(latestVersion);
                return latest > current;
            }
            catch
            {
                return false;
            }
        }

        private async Task DownloadAndInstallUpdateAsync()
        {
            try
            {
                var updatePath = Path.Combine(Path.GetTempPath(), "AccountingApp_Update.zip");
                var extractPath = Path.Combine(Path.GetTempPath(), "AccountingApp_Update");

                // دانلود فایل آپدیت
                MessageBox.Show("در حال دانلود آپدیت...", "آپدیت", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                var response = await _httpClient.GetAsync(_downloadUrl);
                response.EnsureSuccessStatusCode();
                
                using (var fileStream = File.Create(updatePath))
                {
                    await response.Content.CopyToAsync(fileStream);
                }

                // استخراج فایل
                System.IO.Compression.ZipFile.ExtractToDirectory(updatePath, extractPath, true);

                // ایجاد فایل batch برای نصب آپدیت
                var batchPath = CreateUpdateBatch(extractPath);

                // اجرای فایل batch
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = batchPath,
                        UseShellExecute = true,
                        Verb = "runas" // اجرا با دسترسی ادمین
                    }
                };

                MessageBox.Show(
                    "آپدیت دانلود شد. برنامه برای نصب آپدیت بسته خواهد شد.\n" +
                    "لطفاً صبر کنید تا نصب کامل شود.",
                    "نصب آپدیت",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                process.Start();
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"خطا در دانلود آپدیت: {ex.Message}\n\n" +
                    "لطفاً آپدیت را به صورت دستی از GitHub دانلود کنید.",
                    "خطا در آپدیت",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private string CreateUpdateBatch(string extractPath)
        {
            var batchPath = Path.Combine(Path.GetTempPath(), "update_accounting.bat");
            var currentExePath = Process.GetCurrentProcess().MainModule?.FileName ?? "";
            var currentDir = Path.GetDirectoryName(currentExePath) ?? "";

            var batchContent = $@"
@echo off
echo در حال نصب آپدیت...
timeout /t 2 /nobreak > nul

REM بستن برنامه فعلی
taskkill /f /im AccountingApp.exe 2>nul
timeout /t 1 /nobreak > nul

REM کپی فایل‌های جدید
xcopy ""{extractPath}\\*"" ""{currentDir}"" /E /Y /Q

REM پاک کردن فایل‌های موقت
rmdir /s /q ""{extractPath}""
del ""{Path.GetTempPath()}\\AccountingApp_Update.zip""

echo آپدیت با موفقیت نصب شد!
echo در حال راه‌اندازی مجدد برنامه...

REM راه‌اندازی مجدد برنامه
start """" ""{currentExePath}""

REM حذف فایل batch
del ""%~f0""
";

            File.WriteAllText(batchPath, batchContent);
            return batchPath;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class GitHubRelease
    {
        public string tag_name { get; set; } = "";
        public string name { get; set; } = "";
        public string body { get; set; } = "";
        public DateTime published_at { get; set; }
    }
} 