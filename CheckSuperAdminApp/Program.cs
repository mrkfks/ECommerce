using System;
using System.Data;
using Microsoft.Data.Sqlite;

class Program
{
    static void Main()
    {
        string dbRelativePath = "..\\src\\Presentation\\ECommerce.RestApi\\ECommerce.db";
        string dbFullPath = System.IO.Path.GetFullPath(dbRelativePath);
        string currentDir = Environment.CurrentDirectory;
        Console.WriteLine($"[Bilgi] Çalışma dizini: {currentDir}");
        Console.WriteLine($"[Bilgi] Kullanılan veritabanı dosyası: {dbFullPath}");

        if (!System.IO.File.Exists(dbFullPath))
        {
            Console.WriteLine($"[HATA] Veritabanı dosyası bulunamadı: {dbFullPath}");
            return;
        }

        var connectionString = $"Data Source={dbFullPath}";
        try
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();
            Console.WriteLine("[Bilgi] Veritabanı bağlantısı başarılı.");

            // Tablo var mı kontrolü
            var checkTableCmd = connection.CreateCommand();
            checkTableCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Users'";
            var tableResult = checkTableCmd.ExecuteScalar();
            if (tableResult == null)
            {
                Console.WriteLine("[HATA] 'Users' tablosu bulunamadı!");
                return;
            }

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
        catch (Exception ex)
        {
            Console.WriteLine($"[HATA] {ex.Message}");
        }
    }
}
