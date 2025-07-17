using AccountingApp.Database;
using AccountingApp.Forms;
using AccountingApp.Repositories;
using AccountingApp.Models;
using Dapper;
using PdfSharp.Fonts;

namespace AccountingApp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            
            // تنظیم FontResolver برای پشتیبانی از فونت فارسی
            GlobalFontSettings.FontResolver = new CustomFontResolver();
            
            // راه‌اندازی پایگاه داده
            var databaseManager = new DatabaseManager();
            
            // بررسی داده‌های پایگاه
            CheckDatabaseData();
            
            Application.Run(new MainForm());
        }
        
        private static void CheckDatabaseData()
        {
            try
            {
                var databaseManager = new DatabaseManager();
                using var connection = databaseManager.GetConnection();
                
                // بررسی تعداد مشتریان
                var customerCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Customers WHERE IsActive = 1");
                Console.WriteLine($"تعداد مشتریان: {customerCount}");
                
                // بررسی تعداد فروش‌ها
                var saleCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Sales");
                Console.WriteLine($"تعداد فروش‌ها: {saleCount}");
                
                // بررسی تعداد محصولات
                var productCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Products WHERE IsActive = 1");
                Console.WriteLine($"تعداد محصولات: {productCount}");
                
                // نمایش مشتریان
                var customers = connection.Query<dynamic>("SELECT Id, Name, Phone FROM Customers WHERE IsActive = 1");
                Console.WriteLine("مشتریان موجود:");
                foreach (var customer in customers)
                {
                    Console.WriteLine($"ID: {customer.Id}, Name: {customer.Name}, Phone: {customer.Phone}");
                }
                
                // نمایش فروش‌ها
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
    }
} 