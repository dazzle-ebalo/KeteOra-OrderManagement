using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace KeteOra.Orders.Security;

// The single admin login. The username and password hash come from configuration at runtime
// (user-secrets on a developer PC, a Kubernetes Secret in AKS), never from source code.
public class AdminAccount(IConfiguration config)
{
    private static readonly PasswordHasher<object> Hasher = new();
    private static readonly object HashUser = new();

    public string? Username => config["Admin:Username"];
    public string DisplayName => config["Admin:DisplayName"] ?? "Store admin";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(config["Admin:Username"]) &&
        !string.IsNullOrWhiteSpace(config["Admin:PasswordHash"]);

    public bool Verify(string username, string password)
    {
        var expectedUser = config["Admin:Username"];
        var hash = config["Admin:PasswordHash"];
        if (string.IsNullOrWhiteSpace(expectedUser) || string.IsNullOrWhiteSpace(hash))
            return false;

        // Always run the hash check, so a wrong username takes as long as a wrong password
        var passwordOk = Hasher.VerifyHashedPassword(HashUser, hash, password) != PasswordVerificationResult.Failed;
        var userOk = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(username.Trim().ToLowerInvariant()),
            Encoding.UTF8.GetBytes(expectedUser.Trim().ToLowerInvariant()));
        return userOk && passwordOk;
    }

    // Used by: dotnet run --project app -- hash-password
    public static void RunHashPasswordTool()
    {
        var first = ReadHidden("New admin password: ");
        var second = ReadHidden("Type it again: ");
        if (first != second) { Console.WriteLine("The passwords don't match."); return; }
        if (first.Length < 12) { Console.WriteLine("Use at least 12 characters."); return; }
        Console.WriteLine();
        Console.WriteLine("Password hash (store this as Admin:PasswordHash, not the password):");
        Console.WriteLine(Hasher.HashPassword(HashUser, first));
    }

    private static string ReadHidden(string prompt)
    {
        Console.Write(prompt);
        try
        {
            var text = new StringBuilder();
            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Enter) break;
                if (key.Key == ConsoleKey.Backspace) { if (text.Length > 0) text.Length--; continue; }
                if (!char.IsControl(key.KeyChar)) text.Append(key.KeyChar);
            }
            Console.WriteLine();
            return text.ToString();
        }
        catch (InvalidOperationException)
        {
            // Some terminals can't hide input; fall back to a normal read
            return Console.ReadLine() ?? "";
        }
    }
}
