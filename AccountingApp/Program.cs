using AccountingApp.Database;
using AccountingApp.Forms;
using AccountingApp.Repositories;
using AccountingApp.Models;
using AccountingApp.Services;
using Dapper;
using PdfSharp.Fonts;

namespace AccountingApp
{
    internal static class Program
    {
        [STAThread]
        static async Task Main()
        {
            ApplicationConfiguration.Initialize();
            
            // تنظیم COM برای جلوگیری از مشکلات ActiveX
            try
            {
                AccountingApp.Utilities.ActiveXThreadingFix.InitializeComForActiveX();
            }
            catch
            {
                // نادیده گرفتن خطا در صورت عدم موفقیت
            }
            
            GlobalFontSettings.FontResolver = new CustomFontResolver();
            
            var databaseManager = new DatabaseManager();
            
            CheckDatabaseData();
            
            // بررسی آپدیت خودکار
            await CheckForUpdatesAsync();
            
            Application.Run(new MainForm());
            
            // پاکسازی COM
            try
            {
                AccountingApp.Utilities.ActiveXThreadingFix.CleanupCom();
            }
            catch
            {
                // نادیده گرفتن خطا
            }
        }
        
        private static void CheckDatabaseData()
        {
            try
            {
                var databaseManager = new DatabaseManager();
                using var connection = databaseManager.GetConnection();
                
                var customerCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Customers WHERE IsActive = 1");
                Console.WriteLine($"تعداد مشتریان: {customerCount}");
                
                var saleCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Sales");
                Console.WriteLine($"تعداد فروش‌ها: {saleCount}");
                
                var productCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Products WHERE IsActive = 1");
                Console.WriteLine($"تعداد محصولات: {productCount}");
                
                var customers = connection.Query<dynamic>("SELECT Id, Name, Phone FROM Customers WHERE IsActive = 1");
                Console.WriteLine("مشتریان موجود:");
                foreach (var customer in customers)
                {
                    Console.WriteLine($"ID: {customer.Id}, Name: {customer.Name}, Phone: {customer.Phone}");
                }
                
                var sales = connection.Query<dynamic>("SELECT Id, InvoiceNumber, SaleDate, FinalAmount FROM Sales");
                Console.WriteLine("فروش‌های موجود:");
                foreach (var sale in sales)
                {
                    Console.WriteLine($"ID: {sale.Id}, Invoice: {sale.InvoiceNumber}, Date: {sale.SaleDate}, Amount: {sale.FinalAmount}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطا در بررسی پایگاه داده: {ex.Message}");
            }
        }
        
        private static async Task CheckForUpdatesAsync()
        {
            try
            {
                using var updateService = new UpdateService();
                await updateService.CheckForUpdatesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطا در بررسی آپدیت: {ex.Message}");
            }
        }
    }
} 