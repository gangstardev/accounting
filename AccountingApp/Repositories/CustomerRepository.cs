using System.Data.SQLite;
using Dapper;
using AccountingApp.Models;
using AccountingApp.Database;

namespace AccountingApp.Repositories
{
    public class CustomerRepository
    {
        private readonly DatabaseManager _databaseManager;

        public CustomerRepository()
        {
            _databaseManager = new DatabaseManager();
        }

        public List<Customer> GetAll()
        {
            using var connection = _databaseManager.GetConnection();
            var customers = connection.Query<Customer>("SELECT * FROM Customers WHERE IsActive = 1 ORDER BY Name").ToList();
            return customers ?? new List<Customer>();
        }

        public Customer? GetById(int id)
        {
            using var connection = _databaseManager.GetConnection();
            return connection.QueryFirstOrDefault<Customer>("SELECT * FROM Customers WHERE Id = @Id", new { Id = id });
        }

        public int Add(Customer customer)
        {
            using var connection = _databaseManager.GetConnection();
            customer.CreatedDate = DateTime.Now;
            
            var sql = @"
                INSERT INTO Customers (Name, Phone, Address, NationalCode, Email, CreatedDate, IsActive)
                VALUES (@Name, @Phone, @Address, @NationalCode, @Email, @CreatedDate, @IsActive);
                SELECT last_insert_rowid();";
            
            return connection.ExecuteScalar<int>(sql, customer);
        }

        public bool Update(Customer customer)
        {
            using var connection = _databaseManager.GetConnection();
            customer.UpdatedDate = DateTime.Now;
            
            var sql = @"
                UPDATE Customers 
                SET Name = @Name, Phone = @Phone, Address = @Address, 
                    NationalCode = @NationalCode, Email = @Email, UpdatedDate = @UpdatedDate, IsActive = @IsActive
                WHERE Id = @Id";
            
            return connection.Execute(sql, customer) > 0;
        }

        public bool Delete(int id)
        {
            using var connection = _databaseManager.GetConnection();
            return connection.Execute("UPDATE Customers SET IsActive = 0 WHERE Id = @Id", new { Id = id }) > 0;
        }

        public List<Customer> Search(string searchTerm)
        {
            using var connection = _databaseManager.GetConnection();
            var sql = @"
                SELECT * FROM Customers 
                WHERE IsActive = 1 AND (Name LIKE @SearchTerm OR Phone LIKE @SearchTerm OR NationalCode LIKE @SearchTerm)
                ORDER BY Name";
            
            return connection.Query<Customer>(sql, new { SearchTerm = $"%{searchTerm}%" }).ToList();
        }
    }
} 