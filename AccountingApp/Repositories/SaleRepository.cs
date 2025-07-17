using System.Data.SQLite;
using Dapper;
using AccountingApp.Models;
using AccountingApp.Database;

namespace AccountingApp.Repositories
{
    public class SaleRepository
    {
        private readonly DatabaseManager _databaseManager;

        public SaleRepository()
        {
            _databaseManager = new DatabaseManager();
        }

        public List<Sale> GetAll()
        {
            using var connection = _databaseManager.GetConnection();
            var sales = connection.Query<Sale, Customer, Sale>(@"
                SELECT s.*, c.* FROM Sales s 
                INNER JOIN Customers c ON s.CustomerId = c.Id 
                ORDER BY s.SaleDate DESC", 
                (sale, customer) => { sale.Customer = customer; return sale; }).ToList() ?? new List<Sale>();

            foreach (var sale in sales)
            {
                sale.Items = GetSaleItems(sale.Id);
            }

            return sales;
        }

        public Sale? GetById(int id)
        {
            using var connection = _databaseManager.GetConnection();
            var sale = connection.Query<Sale, Customer, Sale>(@"
                SELECT s.*, c.* FROM Sales s 
                INNER JOIN Customers c ON s.CustomerId = c.Id 
                WHERE s.Id = @Id", 
                (sale, customer) => { sale.Customer = customer; return sale; }, 
                new { Id = id }).FirstOrDefault();

            if (sale != null)
            {
                sale.Items = GetSaleItems(sale.Id);
            }

            return sale;
        }

        public Sale? GetByInvoiceNumber(string invoiceNumber)
        {
            using var connection = _databaseManager.GetConnection();
            var sale = connection.Query<Sale, Customer, Sale>(@"
                SELECT s.*, c.* FROM Sales s 
                INNER JOIN Customers c ON s.CustomerId = c.Id 
                WHERE s.InvoiceNumber = @InvoiceNumber", 
                (sale, customer) => { sale.Customer = customer; return sale; }, 
                new { InvoiceNumber = invoiceNumber }).FirstOrDefault();

            if (sale != null)
            {
                sale.Items = GetSaleItems(sale.Id);
            }

            return sale;
        }

        private List<SaleItem> GetSaleItems(int saleId)
        {
            using var connection = _databaseManager.GetConnection();
            var items = connection.Query<SaleItem, Product, SaleItem>(@"
                SELECT si.*, p.* FROM SaleItems si 
                INNER JOIN Products p ON si.ProductId = p.Id 
                WHERE si.SaleId = @SaleId", 
                (item, product) => { item.Product = product; return item; }, 
                new { SaleId = saleId }).ToList();
            return items ?? new List<SaleItem>();
        }

        public int Add(Sale sale)
        {
            using var connection = _databaseManager.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                sale.CreatedDate = DateTime.Now;
                
                // اضافه کردن فروش
                var saleId = connection.ExecuteScalar<int>(@"
                    INSERT INTO Sales (InvoiceNumber, CustomerId, SaleDate, TotalAmount, DiscountAmount, TaxAmount, FinalAmount, Notes, CreatedDate)
                    VALUES (@InvoiceNumber, @CustomerId, @SaleDate, @TotalAmount, @DiscountAmount, @TaxAmount, @FinalAmount, @Notes, @CreatedDate);
                    SELECT last_insert_rowid();", sale, transaction);

                // اضافه کردن آیتم‌های فروش
                foreach (var item in sale.Items)
                {
                    item.SaleId = saleId;
                    connection.Execute(@"
                        INSERT INTO SaleItems (SaleId, ProductId, Quantity, UnitPrice, TotalPrice, DiscountAmount, FinalPrice)
                        VALUES (@SaleId, @ProductId, @Quantity, @UnitPrice, @TotalPrice, @DiscountAmount, @FinalPrice)", 
                        item, transaction);

                    // کاهش موجودی محصول
                    connection.Execute(@"
                        UPDATE Products SET StockQuantity = StockQuantity - @Quantity WHERE Id = @ProductId", 
                        new { item.Quantity, item.ProductId }, transaction);
                }

                transaction.Commit();
                return saleId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool Update(Sale sale)
        {
            using var connection = _databaseManager.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                sale.UpdatedDate = DateTime.Now;

                // بازیابی فروش قبلی برای بازگرداندن موجودی
                var oldSale = GetById(sale.Id);
                if (oldSale != null)
                {
                    foreach (var item in oldSale.Items)
                    {
                        connection.Execute(@"
                            UPDATE Products SET StockQuantity = StockQuantity + @Quantity WHERE Id = @ProductId", 
                            new { item.Quantity, item.ProductId }, transaction);
                    }
                }

                // حذف آیتم‌های قبلی
                connection.Execute("DELETE FROM SaleItems WHERE SaleId = @Id", new { sale.Id }, transaction);

                // به‌روزرسانی فروش
                connection.Execute(@"
                    UPDATE Sales 
                    SET InvoiceNumber = @InvoiceNumber, CustomerId = @CustomerId, SaleDate = @SaleDate,
                        TotalAmount = @TotalAmount, DiscountAmount = @DiscountAmount, TaxAmount = @TaxAmount,
                        FinalAmount = @FinalAmount, Notes = @Notes, UpdatedDate = @UpdatedDate
                    WHERE Id = @Id", sale, transaction);

                // اضافه کردن آیتم‌های جدید
                foreach (var item in sale.Items)
                {
                    item.SaleId = sale.Id;
                    connection.Execute(@"
                        INSERT INTO SaleItems (SaleId, ProductId, Quantity, UnitPrice, TotalPrice, DiscountAmount, FinalPrice)
                        VALUES (@SaleId, @ProductId, @Quantity, @UnitPrice, @TotalPrice, @DiscountAmount, @FinalPrice)", 
                        item, transaction);

                    // کاهش موجودی محصول
                    connection.Execute(@"
                        UPDATE Products SET StockQuantity = StockQuantity - @Quantity WHERE Id = @ProductId", 
                        new { item.Quantity, item.ProductId }, transaction);
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        public bool Delete(int id)
        {
            using var connection = _databaseManager.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // بازیابی فروش برای بازگرداندن موجودی
                var sale = GetById(id);
                if (sale != null)
                {
                    foreach (var item in sale.Items)
                    {
                        connection.Execute(@"
                            UPDATE Products SET StockQuantity = StockQuantity + @Quantity WHERE Id = @ProductId", 
                            new { item.Quantity, item.ProductId }, transaction);
                    }
                }

                // حذف آیتم‌ها
                connection.Execute("DELETE FROM SaleItems WHERE SaleId = @Id", new { Id = id }, transaction);
                
                // حذف فروش
                connection.Execute("DELETE FROM Sales WHERE Id = @Id", new { Id = id }, transaction);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        public List<Sale> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            using var connection = _databaseManager.GetConnection();
            var sales = connection.Query<Sale, Customer, Sale>(@"
                SELECT s.*, c.* FROM Sales s 
                INNER JOIN Customers c ON s.CustomerId = c.Id 
                WHERE DATE(s.SaleDate) BETWEEN DATE(@StartDate) AND DATE(@EndDate)
                ORDER BY s.SaleDate DESC", 
                (sale, customer) => { sale.Customer = customer; return sale; },
                new { StartDate = startDate, EndDate = endDate }).ToList();

            foreach (var sale in sales)
            {
                sale.Items = GetSaleItems(sale.Id);
            }

            return sales;
        }

        public decimal GetTotalSales(DateTime startDate, DateTime endDate)
        {
            using var connection = _databaseManager.GetConnection();
            return connection.ExecuteScalar<decimal>(@"
                SELECT COALESCE(SUM(FinalAmount), 0) FROM Sales 
                WHERE DATE(SaleDate) BETWEEN DATE(@StartDate) AND DATE(@EndDate)",
                new { StartDate = startDate, EndDate = endDate });
        }

        public string GenerateInvoiceNumber()
        {
            using var connection = _databaseManager.GetConnection();
            var lastInvoice = connection.QueryFirstOrDefault<Sale>(
                "SELECT InvoiceNumber FROM Sales ORDER BY Id DESC LIMIT 1");

            if (lastInvoice == null)
                return $"INV-{DateTime.Now:yyyyMMdd}-001";

            var parts = lastInvoice.InvoiceNumber.Split('-');
            if (parts.Length == 3 && parts[1] == DateTime.Now.ToString("yyyyMMdd"))
            {
                var number = int.Parse(parts[2]) + 1;
                return $"INV-{DateTime.Now:yyyyMMdd}-{number:D3}";
            }

            return $"INV-{DateTime.Now:yyyyMMdd}-001";
        }

        public string GenerateTodayInvoiceNumber()
        {
            using var connection = _databaseManager.GetConnection();
            var today = DateTime.Today.ToString("yyyy-MM-dd");
            var count = connection.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM Sales WHERE DATE(SaleDate) = DATE(@Today)", new { Today = today });
            return (count + 1).ToString();
        }
    }
} 