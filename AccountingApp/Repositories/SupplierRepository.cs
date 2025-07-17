using System.Data.SQLite;
using Dapper;
using AccountingApp.Models;
using AccountingApp.Database;

namespace AccountingApp.Repositories
{
    public class SupplierRepository
    {
        private readonly DatabaseManager _databaseManager;

        public SupplierRepository()
        {
            _databaseManager = new DatabaseManager();
        }

        public List<Supplier> GetAll()
        {
            using var connection = _databaseManager.GetConnection();
            // رفع مشکل: اضافه کردن فیلتر IsActive
            var suppliers = connection.Query<Supplier>("SELECT * FROM Suppliers WHERE IsActive = 1 ORDER BY Name").ToList();
            return suppliers ?? new List<Supplier>();
        }

        public Supplier? GetById(int id)
        {
            using var connection = _databaseManager.GetConnection();
            return connection.QueryFirstOrDefault<Supplier>("SELECT * FROM Suppliers WHERE Id = @Id", new { Id = id });
        }

        public int Add(Supplier supplier)
        {
            using var connection = _databaseManager.GetConnection();
            supplier.CreatedDate = DateTime.Now;
            
            var sql = @"
                INSERT INTO Suppliers (Name, Phone, Address, NationalCode, Email, CreatedDate, IsActive)
                VALUES (@Name, @Phone, @Address, @NationalCode, @Email, @CreatedDate, @IsActive);
                SELECT last_insert_rowid();";
            
            return connection.ExecuteScalar<int>(sql, supplier);
        }

        public bool Update(Supplier supplier)
        {
            using var connection = _databaseManager.GetConnection();
            supplier.UpdatedDate = DateTime.Now;
            
            var sql = @"
                UPDATE Suppliers 
                SET Name = @Name, Phone = @Phone, Address = @Address, 
                    NationalCode = @NationalCode, Email = @Email, UpdatedDate = @UpdatedDate, IsActive = @IsActive
                WHERE Id = @Id";
            
            return connection.Execute(sql, supplier) > 0;
        }

        public bool Delete(int id)
        {
            using var connection = _databaseManager.GetConnection();
            return connection.Execute("UPDATE Suppliers SET IsActive = 0 WHERE Id = @Id", new { Id = id }) > 0;
        }

        public List<Supplier> Search(string searchTerm)
        {
            using var connection = _databaseManager.GetConnection();
            var sql = @"
                SELECT * FROM Suppliers 
                WHERE IsActive = 1 AND (Name LIKE @SearchTerm OR Phone LIKE @SearchTerm OR NationalCode LIKE @SearchTerm)
                ORDER BY Name";
            
            return connection.Query<Supplier>(sql, new { SearchTerm = $"%{searchTerm}%" }).ToList();
        }
    }
}