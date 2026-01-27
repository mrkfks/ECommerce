using Microsoft.Data.Sqlite;
using System;

string connectionString = "Data Source=d:/Ömer Kafkas/Projects/ECommerce/src/Presentation/ECommerce.RestApi/ECommerce.db";
using var connection = new SqliteConnection(connectionString);
connection.Open();

Console.WriteLine("--- USERS ---");
using var command = new SqliteCommand("SELECT Id, Username, Email, IsActive FROM Users", connection);
using var reader = command.ExecuteReader();
while (reader.Read())
{
    Console.WriteLine($"ID: {reader["Id"]}, User: {reader["Username"]}, Email: {reader["Email"]}, Active: {reader["IsActive"]}");
}

Console.WriteLine("\n--- ROLES ---");
using var roleCmd = new SqliteCommand("SELECT Id, Name FROM Roles", connection);
using var roleReader = roleCmd.ExecuteReader();
while (roleReader.Read())
{
    Console.WriteLine($"ID: {roleReader["Id"]}, Name: {roleReader["Name"]}");
}

Console.WriteLine("\n--- USER ROLES ---");
using var urCmd = new SqliteCommand("SELECT UserId, RoleId FROM UserRoles", connection);
using var urReader = urCmd.ExecuteReader();
while (urReader.Read())
{
    Console.WriteLine($"UserId: {urReader["UserId"]}, RoleId: {urReader["RoleId"]}");
}
