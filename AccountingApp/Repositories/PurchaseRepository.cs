using System.Data.SQLite;
using Dapper;
using AccountingApp.Models;
using AccountingApp.Database;

namespace AccountingApp.Repositories
{
    public class PurchaseRepository
    {
        private readonly DatabaseManager _databaseManager;

        public PurchaseRepository()
        {
            _databaseManager = new DatabaseManager();
        }

        public List<Purchase> GetAll()
        {
            using var connection = _databaseManager.GetConnection();
            var purchases = connection.Query<Purchase, Supplier, Purchase>(@"
                SELECT p.*, s.* FROM Purchases p 
                INNER JOIN Suppliers s ON p.SupplierId = s.Id 
                ORDER BY p.PurchaseDate DESC", 
                (purchase, supplier) => { purchase.Supplier = supplier; return purchase; }).ToList() ?? new List<Purchase>();

            foreach (var purchase in purchases)
            {
                purchase.Items = GetPurchaseItems(purchase.Id);
            }

            return purchases;
        }

        public Purchase? GetById(int id)
        {
            using var connection = _databaseManager.GetConnection();
            var purchase = connection.Query<Purchase, Supplier, Purchase>(@"
                SELECT p.*, s.* FROM Purchases p 
                INNER JOIN Suppliers s ON p.SupplierId = s.Id 
                WHERE p.Id = @Id", 
                (purchase, supplier) => { purchase.Supplier = supplier; return purchase; }, 
                new { Id = id }).FirstOrDefault();

            if (purchase != null)
            {
                purchase.Items = GetPurchaseItems(purchase.Id);
            }

            return purchase;
        }

        public Purchase? GetByInvoiceNumber(string invoiceNumber)
        {
            using var connection = _databaseManager.GetConnection();
            var purchase = connection.Query<Purchase, Supplier, Purchase>(@"
                SELECT p.*, s.* FROM Purchases p 
                INNER JOIN Suppliers s ON p.SupplierId = s.Id 
                WHERE p.InvoiceNumber = @InvoiceNumber", 
                (purchase, supplier) => { purchase.Supplier = supplier; return purchase; }, 
                new { InvoiceNumber = invoiceNumber }).FirstOrDefault();

            if (purchase != null)
            {
                purchase.Items = GetPurchaseItems(purchase.Id);
            }

            return purchase;
        }

        private List<PurchaseItem> GetPurchaseItems(int purchaseId)
        {
            using var connection = _databaseManager.GetConnection();
            var items = connection.Query<PurchaseItem, Product, PurchaseItem>(@"
                SELECT pi.*, p.* FROM PurchaseItems pi 
                INNER JOIN Products p ON pi.ProductId = p.Id 
                WHERE pi.PurchaseId = @PurchaseId", 
                (item, product) => { item.Product = product; return item; }, 
                new { PurchaseId = purchaseId }).ToList();
            return items ?? new List<PurchaseItem>();
        }

        public int Add(Purchase purchase)
        {
            using var connection = _databaseManager.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                purchase.CreatedDate = DateTime.Now;
                
                // اضافه کردن خرید
                var purchaseId = connection.ExecuteScalar<int>(@"
                    INSERT INTO Purchases (InvoiceNumber, SupplierId, PurchaseDate, TotalAmount, DiscountAmount, TaxAmount, FinalAmount, Notes, CreatedDate)
                    VALUES (@InvoiceNumber, @SupplierId, @PurchaseDate, @TotalAmount, @DiscountAmount, @TaxAmount, @FinalAmount, @Notes, @CreatedDate);
                    SELECT last_insert_rowid();", purchase, transaction);

                // اضافه کردن آیتم‌های خرید
                foreach (var item in purchase.Items)
                {
                    item.PurchaseId = purchaseId;
                    connection.Execute(@"
                        INSERT INTO PurchaseItems (PurchaseId, ProductId, Quantity, UnitPrice, TotalPrice, DiscountAmount, FinalPrice)
                        VALUES (@PurchaseId, @ProductId, @Quantity, @UnitPrice, @TotalPrice, @DiscountAmount, @FinalPrice)", 
                        item, transaction);

                    // افزایش موجودی محصول
                    connection.Execute(@"
                        UPDATE Products SET StockQuantity = StockQuantity + @Quantity WHERE Id = @ProductId", 
                        new { item.Quantity, item.ProductId }, transaction);
                }

                transaction.Commit();
                return purchaseId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool Update(Purchase purchase)
        {
            using var connection = _databaseManager.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                purchase.UpdatedDate = DateTime.Now;

                // بازیابی خرید قبلی برای بازگرداندن موجودی
                var oldPurchase = GetById(purchase.Id);
                if (oldPurchase != null)
                {
                    foreach (var item in oldPurchase.Items)
                    {
                        connection.Execute(@"
                            UPDATE Products SET StockQuantity = StockQuantity - @Quantity WHERE Id = @ProductId", 
                            new { item.Quantity, item.ProductId }, transaction);
                    }
                }

                // حذف آیتم‌های قبلی
                connection.Execute("DELETE FROM PurchaseItems WHERE PurchaseId = @Id", new { purchase.Id }, transaction);

                // به‌روزرسانی خرید
                connection.Execute(@"
                    UPDATE Purchases 
                    SET InvoiceNumber = @InvoiceNumber, SupplierId = @SupplierId, PurchaseDate = @PurchaseDate,
                        TotalAmount = @TotalAmount, DiscountAmount = @DiscountAmount, TaxAmount = @TaxAmount,
                        FinalAmount = @FinalAmount, Notes = @Notes, UpdatedDate = @UpdatedDate
                    WHERE Id = @Id", purchase, transaction);

                // اضافه کردن آیتم‌های جدید
                foreach (var item in purchase.Items)
                {
                    item.PurchaseId = purchase.Id;
                    connection.Execute(@"
                        INSERT INTO PurchaseItems (PurchaseId, ProductId, Quantity, UnitPrice, TotalPrice, DiscountAmount, FinalPrice)
                        VALUES (@PurchaseId, @ProductId, @Quantity, @UnitPrice, @TotalPrice, @DiscountAmount, @FinalPrice)", 
                        item, transaction);

                    // افزایش موجودی محصول
                    connection.Execute(@"
                        UPDATE Products SET StockQuantity = StockQuantity + @Quantity WHERE Id = @ProductId", 
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
                // بازیابی خرید برای بازگرداندن موجودی
                var purchase = GetById(id);
                if (purchase != null)
                {
                    foreach (var item in purchase.Items)
                    {
                        connection.Execute(@"
                            UPDATE Products SET StockQuantity = StockQuantity - @Quantity WHERE Id = @ProductId", 
                            new { item.Quantity, item.ProductId }, transaction);
                    }
                }

                // حذف آیتم‌ها
                connection.Execute("DELETE FROM PurchaseItems WHERE PurchaseId = @Id", new { Id = id }, transaction);
                
                // حذف خرید
                connection.Execute("DELETE FROM Purchases WHERE Id = @Id", new { Id = id }, transaction);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        public List<Purchase> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            using var connection = _databaseManager.GetConnection();
            var purchases = connection.Query<Purchase, Supplier, Purchase>(@"
                SELECT p.*, s.* FROM Purchases p 
                INNER JOIN Suppliers s ON p.SupplierId = s.Id 
                WHERE p.PurchaseDate BETWEEN @StartDate AND @EndDate
                ORDER BY p.PurchaseDate DESC", 
                (purchase, supplier) => { purchase.Supplier = supplier; return purchase; },
                new { StartDate = startDate.ToString("yyyy-MM-dd"), EndDate = endDate.ToString("yyyy-MM-dd") }).ToList();

            foreach (var purchase in purchases)
            {
                purchase.Items = GetPurchaseItems(purchase.Id);
            }

            return purchases;
        }

        public decimal GetTotalPurchases(DateTime startDate, DateTime endDate)
        {
            using var connection = _databaseManager.GetConnection();
            return connection.ExecuteScalar<decimal>(@"
                SELECT COALESCE(SUM(FinalAmount), 0) FROM Purchases 
                WHERE PurchaseDate BETWEEN @StartDate AND @EndDate",
                new { StartDate = startDate.ToString("yyyy-MM-dd"), EndDate = endDate.ToString("yyyy-MM-dd") });
        }

        public string GenerateInvoiceNumber()
        {
            using var connection = _databaseManager.GetConnection();
            var lastInvoice = connection.QueryFirstOrDefault<Purchase>(
                "SELECT InvoiceNumber FROM Purchases ORDER BY Id DESC LIMIT 1");

            if (lastInvoice == null)
                return $"PO-{DateTime.Now:yyyyMMdd}-001";

            var parts = lastInvoice.InvoiceNumber.Split('-');
            if (parts.Length == 3 && parts[1] == DateTime.Now.ToString("yyyyMMdd"))
            {
                var number = int.Parse(parts[2]) + 1;
                return $"PO-{DateTime.Now:yyyyMMdd}-{number:D3}";
            }

            return $"PO-{DateTime.Now:yyyyMMdd}-001";
        }
    }
} 