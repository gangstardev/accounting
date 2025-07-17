using System.Data.SQLite;
using Dapper;
using AccountingApp.Models;

namespace AccountingApp.Database
{
    public class DatabaseManager
    {
        private readonly string _connectionString;

        public DatabaseManager()
        {
            string dbFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database");
            if (!Directory.Exists(dbFolder))
                Directory.CreateDirectory(dbFolder);

            // رفع مشکل: تغییر نام فایل
            string dbPath = Path.Combine(dbFolder, "AccountingDB.db");
            _connectionString = $"Data Source={dbPath};Version=3;";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                using var connection = new SQLiteConnection(_connectionString);
                connection.Open();

                // ایجاد جدول محصولات
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS Products (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Code TEXT UNIQUE,
                        Description TEXT,
                        Category TEXT,
                        Price REAL NOT NULL,
                        CostPrice REAL NOT NULL,
                        StockQuantity INTEGER DEFAULT 0,
                        MinStockLevel INTEGER DEFAULT 10,
                        Unit TEXT,
                        Weight REAL DEFAULT 0,
                        CreatedDate TEXT DEFAULT CURRENT_TIMESTAMP,
                        UpdatedDate TEXT,
                        IsActive INTEGER DEFAULT 1
                    )");

                // اضافه کردن ستون Weight به جدول موجود (اگر وجود ندارد)
                try
                {
                    connection.Execute("ALTER TABLE Products ADD COLUMN Weight REAL DEFAULT 0");
                }
                catch
                {
                    // ستون از قبل وجود دارد
                }

                // ایجاد جدول مشتریان
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS Customers (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Phone TEXT,
                        Address TEXT,
                        NationalCode TEXT,
                        Email TEXT,
                        CreatedDate TEXT DEFAULT CURRENT_TIMESTAMP,
                        UpdatedDate TEXT,
                        IsActive INTEGER DEFAULT 1
                    )");

                // ایجاد جدول تامین‌کنندگان
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS Suppliers (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Phone TEXT,
                        Address TEXT,
                        NationalCode TEXT,
                        Email TEXT,
                        CreatedDate TEXT DEFAULT CURRENT_TIMESTAMP,
                        UpdatedDate TEXT,
                        IsActive INTEGER DEFAULT 1
                    )");

                // ایجاد جدول فروش
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS Sales (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        InvoiceNumber TEXT NOT NULL UNIQUE,
                        CustomerId INTEGER NOT NULL,
                        SaleDate TEXT NOT NULL,
                        TotalAmount REAL NOT NULL,
                        DiscountAmount REAL DEFAULT 0,
                        TaxAmount REAL DEFAULT 0,
                        FinalAmount REAL NOT NULL,
                        Notes TEXT,
                        CreatedDate TEXT DEFAULT CURRENT_TIMESTAMP,
                        UpdatedDate TEXT,
                        FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
                    )");

                // ایجاد جدول آیتم‌های فروش
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS SaleItems (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        SaleId INTEGER NOT NULL,
                        ProductId INTEGER NOT NULL,
                        Quantity INTEGER NOT NULL,
                        UnitPrice REAL NOT NULL,
                        TotalPrice REAL NOT NULL,
                        DiscountAmount REAL DEFAULT 0,
                        FinalPrice REAL NOT NULL,
                        FOREIGN KEY (SaleId) REFERENCES Sales(Id),
                        FOREIGN KEY (ProductId) REFERENCES Products(Id)
                    )");

                // ایجاد جدول خرید
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS Purchases (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        InvoiceNumber TEXT NOT NULL UNIQUE,
                        SupplierId INTEGER NOT NULL,
                        PurchaseDate TEXT NOT NULL,
                        TotalAmount REAL NOT NULL,
                        DiscountAmount REAL DEFAULT 0,
                        TaxAmount REAL DEFAULT 0,
                        FinalAmount REAL NOT NULL,
                        Notes TEXT,
                        CreatedDate TEXT DEFAULT CURRENT_TIMESTAMP,
                        UpdatedDate TEXT,
                        FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id)
                    )");

                // ایجاد جدول آیتم‌های خرید
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS PurchaseItems (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        PurchaseId INTEGER NOT NULL,
                        ProductId INTEGER NOT NULL,
                        Quantity INTEGER NOT NULL,
                        UnitPrice REAL NOT NULL,
                        TotalPrice REAL NOT NULL,
                        DiscountAmount REAL DEFAULT 0,
                        FinalPrice REAL NOT NULL,
                        FOREIGN KEY (PurchaseId) REFERENCES Purchases(Id),
                        FOREIGN KEY (ProductId) REFERENCES Products(Id)
                    )");

                // حذف شده: InsertSampleData(connection);
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در ایجاد پایگاه داده: {ex.Message}", ex);
            }
        }



        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(_connectionString);
        }
    }
}