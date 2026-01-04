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
    cmdAllUsers.CommandText = "SELECT Id, Username, Email, CompanyId, IsActive, IsDeleted FROM Users ORDER BY Id";

    using (var reader = cmdAllUsers.ExecuteReader())
    {
        Console.WriteLine("{0,-5} {1,-20} {2,-35} {3,-10} {4,-8} {5,-8}", "ID", "Username", "Email", "CompanyId", "Active", "Deleted");
        Console.WriteLine(new string('-', 95));

        while (reader.Read())
        {
            Console.WriteLine("{0,-5} {1,-20} {2,-35} {3,-10} {4,-8} {5,-8}",
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt32(3),
                reader.GetBoolean(4),
                reader.GetBoolean(5));
        }
    }

    // List all roles
    Console.WriteLine("\n=== All Roles ===");
    var cmdRoles = connection.CreateCommand();
    cmdRoles.CommandText = "SELECT Id, Name, Description FROM Roles ORDER BY Id";

    using (var reader = cmdRoles.ExecuteReader())
    {
        Console.WriteLine("{0,-5} {1,-20} {2,-50}", "ID", "Name", "Description");
        Console.WriteLine(new string('-', 75));

        while (reader.Read())
        {
            Console.WriteLine("{0,-5} {1,-20} {2,-50}",
                reader.GetInt32(0),
                reader.GetString(1),
                reader.IsDBNull(2) ? "" : reader.GetString(2));
        }
    }

    // List user roles
    Console.WriteLine("\n=== User Roles ===");
    var cmdUserRoles = connection.CreateCommand();
    cmdUserRoles.CommandText = @"
        SELECT ur.UserId, u.Username, ur.RoleId, ur.RoleName 
        FROM UserRoles ur 
        JOIN Users u ON ur.UserId = u.Id 
        ORDER BY ur.UserId";

    using (var reader = cmdUserRoles.ExecuteReader())
    {
        Console.WriteLine("{0,-8} {1,-20} {2,-8} {3,-20}", "UserId", "Username", "RoleId", "RoleName");
        Console.WriteLine(new string('-', 60));

        while (reader.Read())
        {
            Console.WriteLine("{0,-8} {1,-20} {2,-8} {3,-20}",
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.IsDBNull(3) ? "" : reader.GetString(3));
        }
    }

    // List companies
    Console.WriteLine("\n=== Companies ===");
    var cmdCompanies = connection.CreateCommand();
    cmdCompanies.CommandText = "SELECT Id, Name, IsApproved, IsActive, IsDeleted FROM Companies ORDER BY Id";

    using (var reader = cmdCompanies.ExecuteReader())
    {
        Console.WriteLine("{0,-5} {1,-30} {2,-10} {3,-8} {4,-8}", "ID", "Name", "Approved", "Active", "Deleted");
        Console.WriteLine(new string('-', 70));

        while (reader.Read())
        {
            Console.WriteLine("{0,-5} {1,-30} {2,-10} {3,-8} {4,-8}",
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetBoolean(2),
                reader.GetBoolean(3),
                reader.GetBoolean(4));
        }
    }
}
