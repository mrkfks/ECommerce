using Microsoft.Data.Sqlite;

var dbPath = Path.Combine("src", "Presentation", "ECommerce.RestApi", "ECommerce.db");     
var connectionString = $"Data Source={dbPath}";

if (!File.Exists(dbPath))
{
    Console.WriteLine($"Database file not found at: {dbPath}");
    return;
}

Console.WriteLine($"Database path: {dbPath}\n");

using (var connection = new SqliteConnection(connectionString))
{
    connection.Open();

    // Check for duplicate usernames
    Console.WriteLine("=== Checking for Duplicate Usernames ===");
    var cmdDuplicates = connection.CreateCommand();
    cmdDuplicates.CommandText = @"
        SELECT Username, COUNT(*) as Count 
        FROM Users 
        GROUP BY Username 
        HAVING COUNT(*) > 1";
    
    using (var reader = cmdDuplicates.ExecuteReader())
    {
        bool hasDuplicates = false;
        while (reader.Read())
        {
            hasDuplicates = true;
            Console.WriteLine($"Username: {reader.GetString(0)}, Count: {reader.GetInt64(1)}");
        }
        
        if (!hasDuplicates)
        {
            Console.WriteLine("No duplicate usernames found.");
        }
    }

    // List all users
    Console.WriteLine("\n=== All Users ===");
    var cmdAllUsers = connection.CreateCommand();
    cmdAllUsers.CommandText = "SELECT Id, Username, Email, CompanyId, IsActive FROM Users ORDER BY Id";
    
    using (var reader = cmdAllUsers.ExecuteReader())
    {
        Console.WriteLine("{0,-5} {1,-20} {2,-35} {3,-10} {4,-8}", "ID", "Username", "Email", "CompanyId", "Active");
        Console.WriteLine(new string('-', 85));
        
        while (reader.Read())
        {
            Console.WriteLine("{0,-5} {1,-20} {2,-35} {3,-10} {4,-8}", 
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt32(3),
                reader.GetBoolean(4));
        }
    }
}
