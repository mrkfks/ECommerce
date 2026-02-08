using System;
using System.IO;
using System.Data.SQLite;

class Program
{
    static void Main()
    {
        string dbPath = @"d:\Ömer Kafkas\Projects\ECommerce\src\Presentation\ECommerce.RestApi\ECommerce.db";
        
        if (!File.Exists(dbPath))
        {
            Console.WriteLine($"Database not found at: {dbPath}");
            return;
        }
        
        try
        {
            using (var conn = new SQLiteConnection($"Data Source={dbPath}"))
            {
                conn.Open();
                
                // Check if ReturnRequests table exists
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='ReturnRequests'";
                    var result = cmd.ExecuteScalar();
                    
                    if (result == null)
                    {
                        Console.WriteLine("❌ ReturnRequests table does not exist");
                        
                        // List all tables
                        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name";
                        using (var reader = cmd.ExecuteReader())
                        {
                            Console.WriteLine("\nAvailable tables:");
                            while (reader.Read())
                            {
                                Console.WriteLine($"  - {reader[0]}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("✅ ReturnRequests table exists");
                        
                        // Count records
                        cmd.CommandText = "SELECT COUNT(*) FROM ReturnRequests";
                        var count = cmd.ExecuteScalar();
                        Console.WriteLine($"✅ Total records: {count}");
                        
                        // Show recent records
                        cmd.CommandText = @"
                            SELECT Id, OrderId, ProductId, Reason, Status, RequestDate 
                            FROM ReturnRequests 
                            ORDER BY RequestDate DESC 
                            LIMIT 5";
                        
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                Console.WriteLine("\n📋 Recent Return Requests:");
                                while (reader.Read())
                                {
                                    Console.WriteLine($"  ID: {reader[0]}, Order: {reader[1]}, Product: {reader[2]}, Reason: {reader[3]}, Status: {reader[4]}, Date: {reader[5]}");
                                }
                            }
                            else
                            {
                                Console.WriteLine("⚠️  No return requests found");
                            }
                        }
                    }
                }
                
                conn.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }
}
