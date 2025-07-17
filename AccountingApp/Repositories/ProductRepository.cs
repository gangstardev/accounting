using System.Data.SQLite;
using Dapper;
using AccountingApp.Models;
using AccountingApp.Database;

namespace AccountingApp.Repositories
{
    public class ProductRepository
    {
        private readonly DatabaseManager _databaseManager;

        public ProductRepository()
        {
            _databaseManager = new DatabaseManager();
        }

        public List<Product> GetAll()
        {
            using var connection = _databaseManager.GetConnection();
            var products = connection.Query<Product>("SELECT * FROM Products WHERE IsActive = 1 ORDER BY Name").ToList();
            return products ?? new List<Product>();
        }

        public Product? GetById(int id)
        {
            using var connection = _databaseManager.GetConnection();
            return connection.QueryFirstOrDefault<Product>("SELECT * FROM Products WHERE Id = @Id", new { Id = id });
        }

        public Product? GetByCode(string code)
        {
            using var connection = _databaseManager.GetConnection();
            return connection.QueryFirstOrDefault<Product>("SELECT * FROM Products WHERE Code = @Code AND IsActive = 1", new { Code = code });
        }

        public int Add(Product product)
        {
            try
            {
                using var connection = _databaseManager.GetConnection();
                var sql = @"INSERT INTO Products (Name, Code, Description, Category, Price, CostPrice, 
                           StockQuantity, MinStockLevel, Unit, Weight, CreatedDate, IsActive) 
                           VALUES (@Name, @Code, @Description, @Category, @Price, @CostPrice, 
                           @StockQuantity, @MinStockLevel, @Unit, @Weight, @CreatedDate, @IsActive);
                           SELECT last_insert_rowid();";
                
                product.CreatedDate = DateTime.Now;
                return connection.QuerySingle<int>(sql, product);
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در افزودن محصول: {ex.Message}");
            }
        }

        public bool Update(Product product)
        {
            try
            {
                using var connection = _databaseManager.GetConnection();
                var sql = @"UPDATE Products SET Name = @Name, Code = @Code, Description = @Description, 
                           Category = @Category, Price = @Price, CostPrice = @CostPrice, 
                           StockQuantity = @StockQuantity, MinStockLevel = @MinStockLevel, 
                           Unit = @Unit, Weight = @Weight, UpdatedDate = @UpdatedDate, IsActive = @IsActive 
                           WHERE Id = @Id";
                
                product.UpdatedDate = DateTime.Now;
                return connection.Execute(sql, product) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در به‌روزرسانی محصول: {ex.Message}");
            }
        }

        public bool Delete(int id)
        {
            using var connection = _databaseManager.GetConnection();
            return connection.Execute("UPDATE Products SET IsActive = 0 WHERE Id = @Id", new { Id = id }) > 0;
        }

        public bool UpdateStock(int productId, int quantity)
        {
            using var connection = _databaseManager.GetConnection();
            return connection.Execute("UPDATE Products SET StockQuantity = StockQuantity + @Quantity WHERE Id = @Id", 
                new { Id = productId, Quantity = quantity }) > 0;
        }

        public List<Product> Search(string searchTerm)
        {
            using var connection = _databaseManager.GetConnection();
            var sql = @"
                SELECT * FROM Products 
                WHERE IsActive = 1 AND (Name LIKE @SearchTerm OR Code LIKE @SearchTerm OR Description LIKE @SearchTerm)
                ORDER BY Name";
            
            return connection.Query<Product>(sql, new { SearchTerm = $"%{searchTerm}%" }).ToList();
        }
    }
}