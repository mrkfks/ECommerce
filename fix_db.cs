using Microsoft.Data.Sqlite;
using System;
using BCrypt.Net;

string connectionString = "Data Source=d:/Ömer Kafkas/Projects/ECommerce/src/Presentation/ECommerce.RestApi/ECommerce.db";
using var connection = new SqliteConnection(connectionString);
connection.Open();

// 1. Ensure SuperAdmin role exists
long roleId = 0;
using (var cmd = new SqliteCommand("SELECT Id FROM Roles WHERE Name = 'SuperAdmin'", connection))
{
    var result = cmd.ExecuteScalar();
    if (result == null)
    {
        using var insertRole = new SqliteCommand("INSERT INTO Roles (Name, Description, CreatedAt, UpdatedAt, IsDeleted) VALUES ('SuperAdmin', 'System Admin', datetime('now'), datetime('now'), 0); SELECT last_insert_rowid();", connection);
        roleId = (long)insertRole.ExecuteScalar();
        Console.WriteLine($"Created SuperAdmin role with ID: {roleId}");
    }
    else
    {
        roleId = (long)result;
        Console.WriteLine($"Found SuperAdmin role with ID: {roleId}");
    }
}

// 2. Ensure Company exists and is approved
long companyId = 0;
using (var cmd = new SqliteCommand("SELECT Id FROM Companies LIMIT 1", connection))
{
    var result = cmd.ExecuteScalar();
    if (result == null)
    {
        using var insertComp = new SqliteCommand("INSERT INTO Companies (Name, Address, Phone, Email, TaxNumber, ContactPerson, ContactPhone, AdminEmail, IsApproved, CreatedAt, UpdatedAt, IsDeleted) VALUES ('Default', 'Addr', '123', 'admin@comp.com', '123', 'Admin', '123', 'admin@comp.com', 1, datetime('now'), datetime('now'), 0); SELECT last_insert_rowid();", connection);
        companyId = (long)insertComp.ExecuteScalar();
        Console.WriteLine($"Created Company with ID: {companyId}");
    }
    else
    {
        companyId = (long)result;
        using var updateComp = new SqliteCommand($"UPDATE Companies SET IsApproved = 1 WHERE Id = {companyId}", connection);
        updateComp.ExecuteNonQuery();
        Console.WriteLine($"Found Company with ID: {companyId} and ensured it is approved.");
    }
}

// 3. Ensure SuperAdmin user exists with correct password
string hash = BCrypt.Net.BCrypt.HashPassword("Admin123!");
long userId = 0;
using (var cmd = new SqliteCommand("SELECT Id FROM Users WHERE Email = 'admin@ecommerce.com'", connection))
{
    var result = cmd.ExecuteScalar();
    if (result == null)
    {
        using var insertUser = new SqliteCommand("INSERT INTO Users (CompanyId, Username, Email, PasswordHash, FirstName, LastName, PhoneNumber, IsActive, CreatedAt, UpdatedAt, IsDeleted) VALUES (@cid, 'superadmin', 'admin@ecommerce.com', @hash, 'Super', 'Admin', '555', 1, datetime('now'), datetime('now'), 0); SELECT last_insert_rowid();", connection);
        insertUser.Parameters.AddWithValue("@cid", companyId);
        insertUser.Parameters.AddWithValue("@hash", hash);
        userId = (long)insertUser.ExecuteScalar();
        Console.WriteLine($"Created SuperAdmin user with ID: {userId}");
    }
    else
    {
        userId = (long)result;
        using var updateUser = new SqliteCommand("UPDATE Users SET PasswordHash = @hash, IsActive = 1 WHERE Id = @id", connection);
        updateUser.Parameters.AddWithValue("@hash", hash);
        updateUser.Parameters.AddWithValue("@id", userId);
        updateUser.ExecuteNonQuery();
        Console.WriteLine($"Found and updated SuperAdmin user with ID: {userId}");
    }
}

// 4. Ensure User-Role mapping
using (var cmd = new SqliteCommand("SELECT COUNT(*) FROM UserRoles WHERE UserId = @uid AND RoleId = @rid", connection))
{
    cmd.Parameters.AddWithValue("@uid", userId);
    cmd.Parameters.AddWithValue("@rid", roleId);
    var count = (long)cmd.ExecuteScalar();
    if (count == 0)
    {
        using var insertUR = new SqliteCommand("INSERT INTO UserRoles (UserId, RoleId, RoleName, CreatedAt, UpdatedAt, IsDeleted) VALUES (@uid, @rid, 'SuperAdmin', datetime('now'), datetime('now'), 0)", connection);
        insertUR.Parameters.AddWithValue("@uid", userId);
        insertUR.Parameters.AddWithValue("@rid", roleId);
        insertUR.ExecuteNonQuery();
        Console.WriteLine("Created User-Role mapping.");
    }
    else
    {
        Console.WriteLine("User-Role mapping already exists.");
    }
}

Console.WriteLine("Database fix completed.");
