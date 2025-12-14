using Microsoft.Data.Sqlite;

var dbPath = Path.Combine("src", "Presentation", "ECommerce.RestApi", "ECommerce.db");     
var connectionString = $"Data Source={dbPath}";

if (!File.Exists(dbPath))
{
    Console.WriteLine("Database file not found!");
    return;
}

using (var connection = new SqliteConnection(connectionString))
{
    connection.Open();

    var emailToDelete = "omerkafkas55@gmail.com"; // Email from screenshot

    var cmdCheck = connection.CreateCommand();
    cmdCheck.CommandText = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
    var pEmail = cmdCheck.CreateParameter();
    pEmail.ParameterName = "@Email";
    pEmail.Value = emailToDelete;
    cmdCheck.Parameters.Add(pEmail);
    
    var count = (long)cmdCheck.ExecuteScalar();
    
    if (count > 0)
    {
        Console.WriteLine($"Found user with email {emailToDelete}. Deleting...");
        
        var cmdDelete = connection.CreateCommand();
        cmdDelete.CommandText = "DELETE FROM Users WHERE Email = @Email";
        var pDelete = cmdDelete.CreateParameter();
        pDelete.ParameterName = "@Email";
        pDelete.Value = emailToDelete;
        cmdDelete.Parameters.Add(pDelete);
        cmdDelete.ExecuteNonQuery();
        
        Console.WriteLine("User deleted successfully.");
    }
    else
    {
        Console.WriteLine("User not found.");
    }
}
