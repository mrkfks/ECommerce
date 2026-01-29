using System;
using System.Data;
using Microsoft.Data.Sqlite;

class Program
{
    static void Main()
    {
        var connectionString = "Data Source=ECommerce.db";
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = @"SELECT Id, Username, Email FROM Users WHERE Username = 'superadmin' OR Email = 'superadmin@ecommerce.com'";
        using var reader = command.ExecuteReader();
        if (!reader.HasRows)
        {
            Console.WriteLine("Kullanıcı bulunamadı.");
        }
        else
        {
            while (reader.Read())
            {
                Console.WriteLine($"Id: {reader["Id"]}, Username: {reader["Username"]}, Email: {reader["Email"]}");
            }
        }
    }
}
