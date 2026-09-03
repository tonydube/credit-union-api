using CreditUnionApi.Models;

namespace CreditUnionApi.Data;

public static class SeedData
{
    public static List<Member> Members = new()
    {
        new Member("m1", "Jane", "Doe", "jane.doe@email.com", new DateTime(2018, 3, 15)),
        new Member("m2", "John", "Smith", "john.smith@email.com", new DateTime(2020, 7, 22))
    };

    public static List<Account> Accounts = new()
    {
        new Account("a1", "m1", "checking", 2450.75m),
        new Account("a2", "m1", "savings", 12000.00m),
        new Account("a3", "m2", "checking", 850.50m),
        new Account("a4", "m2", "savings", 5200.00m)
    };

    public static List<Transaction> Transactions = new()
    {
        new Transaction("t1", "a1", "credit", 1500.00m, "Direct deposit - payroll", new DateTime(2024, 1, 15)),
        new Transaction("t2", "a1", "debit", 45.99m, "Netflix subscription", new DateTime(2024, 1, 16)),
        new Transaction("t3", "a1", "debit", 120.00m, "Grocery store", new DateTime(2024, 1, 17)),
        new Transaction("t4", "a2", "credit", 500.00m, "Transfer from checking", new DateTime(2024, 1, 17)),
        new Transaction("t5", "a3", "credit", 2200.00m, "Direct deposit - payroll", new DateTime(2024, 1, 15)),
        new Transaction("t6", "a3", "debit", 900.00m, "Rent payment", new DateTime(2024, 1, 16))
    };

    // Simple fake auth - username is email, password is "password" for everyone
    public static Dictionary<string, string> Users = new()
    {
        { "jane.doe@email.com", "m1" },
        { "john.smith@email.com", "m2" }
    };
}
