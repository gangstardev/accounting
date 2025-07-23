using System.Data.SQLite;
using Dapper;
using AccountingApp.Database;
using AccountingApp.Models;

namespace AccountingApp.Utilities
{
    public static class DatabaseDiagnostic
    {
        public static void CheckProductCodes()
        {
            try
            {
                using var connection = new DatabaseManager().GetConnection();
                
                // بررسی تعداد کل محصولات
                var totalProducts = connection.QuerySingle<int>("SELECT COUNT(*) FROM Products WHERE IsActive = 1");
                
                // بررسی محصولات بدون کد
                var productsWithoutCode = connection.Query<Product>("SELECT Id, Name, Code FROM Products WHERE (Code IS NULL OR Code = '') AND IsActive = 1").ToList();
                
                // بررسی کدهای تکراری
                var duplicateCodes = connection.Query<dynamic>(@"
                    SELECT Code, COUNT(*) as Count 
                    FROM Products 
                    WHERE Code IS NOT NULL AND Code != '' AND IsActive = 1 
                    GROUP BY Code 
                    HAVING COUNT(*) > 1").ToList();

                var message = $"وضعیت کدهای محصول:\n\n";
                message += $"تعداد کل محصولات فعال: {totalProducts}\n";
                message += $"محصولات بدون کد: {productsWithoutCode.Count}\n";
                message += $"کدهای تکراری: {duplicateCodes.Count}\n\n";

                if (productsWithoutCode.Count > 0)
                {
                    message += "محصولات بدون کد:\n";
                    foreach (var product in productsWithoutCode)
                    {
                        message += $"ID: {product.Id} - نام: {product.Name}\n";
                    }
                    message += "\n";
                }

                if (duplicateCodes.Count > 0)
                {
                    message += "کدهای تکراری:\n";
                    foreach (var duplicate in duplicateCodes)
                    {
                        message += $"کد: {duplicate.Code} - تعداد: {duplicate.Count}\n";
                    }
                }

                MessageBox.Show(message, "تشخیص پایگاه داده", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در تشخیص پایگاه داده: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void ShowProductDetails(string code)
        {
            try
            {
                using var connection = new DatabaseManager().GetConnection();
                var products = connection.Query<Product>("SELECT * FROM Products WHERE Code = @Code", new { Code = code }).ToList();

                if (products.Count == 0)
                {
                    MessageBox.Show($"هیچ محصولی با کد '{code}' یافت نشد.", "اطلاعات", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var message = $"جزئیات محصولات با کد '{code}':\n\n";
                foreach (var product in products)
                {
                    message += $"ID: {product.Id}\n";
                    message += $"نام: {product.Name}\n";
                    message += $"کد: {product.Code}\n";
                    message += $"فعال: {(product.IsActive ? "بله" : "خیر")}\n";
                    message += $"تاریخ ایجاد: {product.CreatedDate}\n";
                    message += "---\n";
                }

                MessageBox.Show(message, "جزئیات محصول", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در نمایش جزئیات محصول: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 