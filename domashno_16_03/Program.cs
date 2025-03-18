using System;
using System.Data;
using Microsoft.Data.SqlClient;

class Program
{
    static string masterConnectionString = "Server=HP-VESELIN\\SQLEXPRESS2019;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";
    static string testDbConnectionString = "Server=HP-VESELIN\\SQLEXPRESS2019;Database=TestDb;Trusted_Connection=True;TrustServerCertificate=True;";

    static void Main()
    {
        EnsureDatabaseExists();  // Проверява и създава базата, ако я няма
        CreateTablesIfNotExist(); // Проверява и създава таблиците, ако ги няма

        InsertProduct("Лаптоп", 1200.50m);
        InsertProduct("Телефон", 800.00m);

        BuyProduct("Веселин", 1);
        BuyProduct("Иван", 1);
        BuyProduct("Мария", 2);

        QueryProductPurchases();

        //DeleteProduct(2);


    }

    static void EnsureDatabaseExists()
    {
        using (SqlConnection connection = new SqlConnection(masterConnectionString))
        {
            connection.Open();
            string checkDbQuery = "SELECT COUNT(*) FROM sys.databases WHERE name = 'TestDb'";

            using (SqlCommand command = new SqlCommand(checkDbQuery, connection))
            {
                int count = (int)command.ExecuteScalar();// с това взимам стойност от заявката
                if (count == 0)
                {
                    string createDbQuery = "CREATE DATABASE TestDb";
                    using (SqlCommand createCommand = new SqlCommand(createDbQuery, connection))
                    {
                        createCommand.ExecuteNonQuery();
                        Console.WriteLine("Базата данни 'TestDb' беше създадена!");
                    }
                }
                else
                {
                    Console.WriteLine("Базата 'TestDb' вече съществува.");
                }
            }
        }
    }

    static void CreateTablesIfNotExist()
    {
        using (SqlConnection connection = new SqlConnection(testDbConnectionString))
        {
            connection.Open();
            string createTablesQuery = @"
                IF OBJECT_ID('Products', 'U') IS NULL
                BEGIN
                    CREATE TABLE Products (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Name NVARCHAR(100) NOT NULL,
                        Price DECIMAL(10,2) NOT NULL
                    );
                END

                IF OBJECT_ID('Customers', 'U') IS NULL
                BEGIN
                    CREATE TABLE Customers (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Name NVARCHAR(100) NOT NULL,
                        ProductId INT FOREIGN KEY REFERENCES Products(Id) ON DELETE CASCADE
                    );
                END";

            using (SqlCommand command = new SqlCommand(createTablesQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        Console.WriteLine("Таблиците са проверени и създадени, ако не са съществували.");
    }

    static void InsertProduct(string name, decimal price)
    {
        using (SqlConnection connection = new SqlConnection(testDbConnectionString))
        {
            connection.Open();
            string query = "INSERT INTO Products (Name, Price) VALUES (@Name, @Price);";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Price", price);
                command.ExecuteNonQuery();
            }
        }

        Console.WriteLine($"Продукт \"{name}\" беше добавен успешно!");
    }

    static void BuyProduct(string customerName, int productId)
    {
        using (SqlConnection connection = new SqlConnection(testDbConnectionString))
        {
            connection.Open();

            // Проверяваме дали продуктът съществува
            string checkProductQuery = "SELECT COUNT(*) FROM Products WHERE Id = @ProductId";
            using (SqlCommand checkCommand = new SqlCommand(checkProductQuery, connection))
            {
                checkCommand.Parameters.AddWithValue("@ProductId", productId);
                int count = (int)checkCommand.ExecuteScalar();
                if (count == 0)
                {
                    Console.WriteLine($" Продукт с ID {productId} не съществува!");
                    return;
                }
            }

            // Добавяме клиента
            string query = "INSERT INTO Customers (Name, ProductId) VALUES (@Name, @ProductId)";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Name", customerName);
                command.Parameters.AddWithValue("@ProductId", productId);
                command.ExecuteNonQuery();
            }
        }

        Console.WriteLine($"Клиент \"{customerName}\" закупи продукта с ID {productId}.");
    }

    static void QueryProductPurchases()
    {
        using (SqlConnection connection = new SqlConnection(testDbConnectionString))
        {
            connection.Open();
            string query = @"
                SELECT p.Name AS ProductName, COUNT(c.Id) AS BuyerCount
                FROM Products p
                LEFT JOIN Customers c ON p.Id = c.ProductId
                GROUP BY p.Name;";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    Console.WriteLine("\nПродукти и брой купувачи:");
                    while (reader.Read())
                    {
                        Console.WriteLine($"Продукт: {reader["ProductName"]}, Купувачи: {reader["BuyerCount"]}");
                    }
                }
            }
        }
    }
    static void DeleteProduct(int id)
    {
        using (SqlConnection connection = new SqlConnection(testDbConnectionString))
        {
            connection.Open();
            string query = "DELETE FROM Products WHERE Id = @Id";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                command.ExecuteNonQuery();
            }
        }
    }
}
