using System;
class Program {
    static void Main() {
        var hash = BCrypt.Net.BCrypt.HashPassword("Customer@123", 11);
        Console.WriteLine(hash);
    }
}
