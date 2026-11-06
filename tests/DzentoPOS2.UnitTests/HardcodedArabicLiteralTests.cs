using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

namespace DzentoPOS2.UnitTests;

/// <summary>
/// Item 4 guard: every user/developer-visible string lives in
/// <c>AppStrings</c>. No production C# file may contain an Arabic string
/// literal, and all four resource files must stay in sync with no empty
/// values.
/// </summary>
public sealed class HardcodedArabicLiteralTests
{
    private static readonly string[] ProductionProjects =
    [
        "DzentoPOS2",
        "DzentoPOS2.App",
        "DzentoPOS2.Core",
        "DzentoPOS2.Data",
        "DzentoPOS2.Shared",
        "DzentoPOS2.Localization",
        "EZControls"
    ];

    private static readonly string[] ResourceFiles =
    [
        "AppStrings.resx",
        "AppStrings.en.resx",
        "AppStrings.ar.resx",
        "AppStrings.fr.resx"
    ];

    private static readonly Regex StringLiteral =
        new("\"([^\"\\\\]|\\\\.)*\"", RegexOptions.Compiled);

    private static readonly Regex Arabic =
        new("[\\u0600-\\u06FF\\u0750-\\u077F\\u08A0-\\u08FF\\uFB50-\\uFDFF\\uFE70-\\uFEFF]", RegexOptions.Compiled);

    [Fact]
    public void Production_source_contains_no_hardcoded_arabic_literals()
    {
        var root = FindRepositoryRoot();
        var offenders = new List<string>();

        foreach (var project in ProductionProjects)
        {
            var directory = Path.Combine(root, project);

            if (!Directory.Exists(directory))
                continue;

            foreach (var file in Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(root, file);

                if (IsBuildOrMigrationFile(relative))
                    continue;

                var text = File.ReadAllText(file);

                foreach (Match match in StringLiteral.Matches(text))
                {
                    if (!Arabic.IsMatch(match.Value))
                        continue;

                    var line = text[..match.Index].Count(character => character == '\n') + 1;
                    offenders.Add($"{relative}:{line}");
                }
            }
        }

        Assert.True(
            offenders.Count == 0,
            "Hardcoded Arabic literals found (move them to AppStrings):\n" +
            string.Join("\n", offenders));
    }

    [Fact]
    public void Every_resource_file_has_the_same_keys_and_no_empty_values()
    {
        var root = FindRepositoryRoot();
        var resources = Path.Combine(root, "DzentoPOS2.Localization", "Resources");

        var sets = ResourceFiles.ToDictionary(
            file => file,
            file => ReadResource(Path.Combine(resources, file)),
            StringComparer.Ordinal);

        var baseline = sets["AppStrings.en.resx"];

        foreach (var (file, set) in sets)
        {
            var missing = baseline.Keys.Except(set.Keys).ToList();
            var extra = set.Keys.Except(baseline.Keys).ToList();

            Assert.True(missing.Count == 0, $"{file}: missing keys: {string.Join(", ", missing)}");
            Assert.True(extra.Count == 0, $"{file}: extra keys: {string.Join(", ", extra)}");

            var empty = set.Where(pair => string.IsNullOrWhiteSpace(pair.Value))
                .Select(pair => pair.Key)
                .ToList();

            Assert.True(empty.Count == 0, $"{file}: empty values: {string.Join(", ", empty)}");
        }
    }

    [Theory]
    [InlineData("ModuleGrocery")]
    [InlineData("RoleNotFound")]
    [InlineData("AmountCannotBeNegative")]
    [InlineData("VariantCreatedFormat")]
    [InlineData("CurrencySymbolSar")]
    public void Newly_localized_keys_are_real_translations(string key)
    {
        var root = FindRepositoryRoot();
        var resources = Path.Combine(root, "DzentoPOS2.Localization", "Resources");

        var arabic = ReadResource(Path.Combine(resources, "AppStrings.ar.resx"));
        var english = ReadResource(Path.Combine(resources, "AppStrings.en.resx"));

        Assert.True(arabic.TryGetValue(key, out var arabicValue), $"ar.resx is missing '{key}'.");
        Assert.True(english.ContainsKey(key), $"en.resx is missing '{key}'.");
        Assert.Contains(arabicValue!, character => character is >= '\u0600' and <= '\u06FF');
    }

    private static Dictionary<string, string> ReadResource(string path)
    {
        var document = XDocument.Load(path, LoadOptions.PreserveWhitespace);

        return document.Root!
            .Elements("data")
            .Where(element => element.Attribute("name") is not null)
            .ToDictionary(
                element => element.Attribute("name")!.Value,
                element => element.Element("value")?.Value ?? string.Empty,
                StringComparer.Ordinal);
    }

    private static bool IsBuildOrMigrationFile(string relativePath)
    {
        var normalized = relativePath.Replace('\\', '/');

        return normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/Migrations/", StringComparison.OrdinalIgnoreCase);
    }

    private static string FindRepositoryRoot([CallerFilePath] string callerPath = "")
    {
        var directory = new DirectoryInfo(Path.GetDirectoryName(callerPath)!);

        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "DzentoPOS2.Core")) &&
                Directory.Exists(Path.Combine(directory.FullName, "DzentoPOS2.Localization")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Repository root not found.");
    }
}