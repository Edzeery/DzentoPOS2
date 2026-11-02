using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace DzentoPOS2.UnitTests;

/// <summary>
/// Regression guard: ensures that service files that perform write operations
/// contain IAuthorizationGuard.DemandAsync calls. If a new service is added
/// without guards, this test should be updated to include it.
/// </summary>
public class PermissionGuardRegressionTests
{
    private static readonly string[] GuardedServiceFiles =
    {
        "CashSessionService.cs",
        "SaleService.cs",
        "PurchaseService.cs",
        "ExpenseService.cs",
        "CustomerService.cs",
        "SupplierService.cs",
        "StockService.cs",
        "BackupService.cs",
        "UserService.cs",
        "RoleService.cs",
        "ProductService.cs",
    };

    [Fact]
    public void Guarded_services_contain_DemandAsync_calls()
    {
        string repoRoot = FindRepoRoot();
        string appServicesDir = Path.Combine(repoRoot, "DzentoPOS2.App", "Services");

        foreach (string fileName in GuardedServiceFiles)
        {
            string? filePath = FindFile(appServicesDir, fileName);
            Assert.True(filePath is not null, $"Service file not found: {fileName}");

            string content = File.ReadAllText(filePath!);
            Assert.Contains("DemandAsync", content,
                StringComparison.Ordinal);
        }
    }

    [Fact]
    public void No_empty_catch_blocks_in_production_code()
    {
        string repoRoot = FindRepoRoot();
        string[] searchDirs =
        {
            Path.Combine(repoRoot, "DzentoPOS2"),
            Path.Combine(repoRoot, "DzentoPOS2.App"),
            Path.Combine(repoRoot, "DzentoPOS2.Data"),
            Path.Combine(repoRoot, "DzentoPOS2.Core"),
        };

        foreach (string dir in searchDirs)
        {
            if (!Directory.Exists(dir)) continue;

            foreach (string file in Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories))
            {
                string content = File.ReadAllText(file);
                // Check for empty catch blocks: catch { } or catch { } on one line
                Assert.DoesNotContain("catch { }", content);
                Assert.DoesNotContain("catch{}", content);
            }
        }
    }

    private static string? FindFile(string root, string fileName)
    {
        if (!Directory.Exists(root)) return null;
        return Directory.GetFiles(root, fileName, SearchOption.AllDirectories).FirstOrDefault();
    }

    private static string FindRepoRoot()
    {
        string dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir, "DzentoPOS2.sln")))
                return dir;
            dir = Path.GetDirectoryName(dir)!;
        }
        // Fallback: walk up from the test assembly
        dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir, "DzentoPOS2.sln")))
                return dir;
            var parent = Path.GetDirectoryName(dir);
            if (parent == dir) break;
            dir = parent!;
        }
        throw new InvalidOperationException("Could not find repo root.");
    }
}