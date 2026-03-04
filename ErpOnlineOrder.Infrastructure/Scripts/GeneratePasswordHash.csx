// Chay file nay trong C# Interactive hoac tao Console App de lay BCrypt hash
// File: GeneratePasswordHash.csx

// Cach 1: Chay trong Visual Studio C# Interactive
// Tools > NuGet Package Manager > Package Manager Console
// Install-Package BCrypt.Net-Next
// Sau do copy code nay vao C# Interactive Window

/*
using BCrypt.Net;
var password = "Admin@123";
var hash = BCrypt.HashPassword(password);
Console.WriteLine($"Password: {password}");
Console.WriteLine($"Hash: {hash}");
Console.WriteLine($"Verify: {BCrypt.Verify(password, hash)}");
*/

// Cach 2: Tao Console App moi
// dotnet new console -n GenerateHash
// dotnet add package BCrypt.Net-Next
// Copy code sau vao Program.cs:

/*
using BCrypt.Net;

var password = "Admin@123";
var hash = BCrypt.Net.BCrypt.HashPassword(password);

Console.WriteLine("========================================");
Console.WriteLine("GENERATE BCRYPT HASH");
Console.WriteLine("========================================");
Console.WriteLine($"Password: {password}");
Console.WriteLine($"Hash: {hash}");
Console.WriteLine($"Hash Length: {hash.Length}");
Console.WriteLine($"Verify: {BCrypt.Net.BCrypt.Verify(password, hash)}");
Console.WriteLine("========================================");
Console.WriteLine();
Console.WriteLine("Copy hash nay vao SQL:");
Console.WriteLine($"UPDATE Users SET Password = '{hash}' WHERE Username = 'admin';");
*/

// Cach 3: Dung LINQPad
// Paste code sau va chay:
/*
void Main()
{
    var password = "Admin@123";
    var hash = BCrypt.Net.BCrypt.HashPassword(password);
    hash.Dump("BCrypt Hash for Admin@123");
}
*/
