using System.Data.SQLite;
using Dapper;
using AccountingApp.Database;
using AccountingApp.Models;

namespace AccountingApp.Utilities
{
    public static class ProductCodeChecker
    {
        public static List<Product> GetAllProductsWithCodes()
        {
            using var connection = new DatabaseManager().GetConnection();
            return connection.Query<Product>("SELECT Id, Name, Code FROM Products WHERE IsActive = 1 ORDER BY Code").ToList();
        }

        public static bool IsCodeExists(string code)
        {
            using var connection = new DatabaseManager().GetConnection();
            var product = connection.QueryFirstOrDefault<Product>("SELECT Id FROM Products WHERE Code = @Code AND IsActive = 1", new { Code = code });
            return product != null;
        }

        public static Product? GetProductByCode(string code)
        {
            using var connection = new DatabaseManager().GetConnection();
            return connection.QueryFirstOrDefault<Product>("SELECT * FROM Products WHERE Code = @Code AND IsActive = 1", new { Code = code });
        }

        public static void ShowExistingCodes()
        {
            var products = GetAllProductsWithCodes();
            if (products.Count == 0)
            {
                MessageBox.Show("هیچ محصولی در پایگاه داده یافت نشد.", "اطلاعات", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var message = "کدهای موجود در سیستم:\n\n";
            foreach (var product in products)
            {
                message += $"کد: {product.Code} - نام: {product.Name}\n";
            }

            MessageBox.Show(message, "کدهای موجود", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
} 