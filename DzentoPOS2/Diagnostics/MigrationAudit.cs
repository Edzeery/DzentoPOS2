using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DzentoPOS2.Core.Configuration;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Seed;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DzentoPOS2.WinForms.Diagnostics;

/// <summary>
/// Item 6 evidence generator. It proves that running the real startup
/// migration path (<see cref="DbInitializer.InitializeAsync"/>) against a copy
/// of a live database never loses data.
/// <para>
/// The live database is never opened for writing. The audit copies the file
/// (plus its <c>-wal</c>/<c>-shm</c> siblings), checkpoints the write-ahead log
/// into the copy, snapshots the copy (integrity, foreign keys, schema, per-row
/// content hashes keyed by primary key, migration history), runs the real
/// initializer against the copy, then snapshots again and reports:
/// </para>
/// <list type="bullet">
///   <item>tables or columns removed (must be none);</item>
///   <item>rows removed (must be none);</item>
///   <item>rows changed and rows added (reported, and explained);</item>
///   <item>migration history entries removed (must be none);</item>
///   <item>usage counts of the legacy inventory columns the removal plan
///   targets.</item>
/// </list>
/// <para>
/// Run with <c>DzentoPOS2.WinForms.exe --migration-audit</c>. Options:
/// <c>--source &lt;db&gt;</c>, <c>--work &lt;dir&gt;</c>, <c>--keep</c>.
/// </para>
/// </summary>
public static class MigrationAudit
{
    private const int MaxDetail = 100;

    public static int Run(string[] args)
    {
        string source = GetOption(args, "--source") ?? AppPaths.DefaultDatabasePath;
        string work = GetOption(args, "--work")
            ?? Path.Combine(
                Path.GetTempPath(),
                "DzentoPOS2.MigrationAudit",
                DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        bool keep = args.Any(a => string.Equals(a, "--keep", StringComparison.OrdinalIgnoreCase));

        if (!File.Exists(source))
        {
            Console.Error.WriteLine($"Migration audit: source database not found: {source}");
            return 2;
        }

        Directory.CreateDirectory(work);
        string copyPath = Path.Combine(work, Path.GetFileName(source));

        bool sourceHadWal = File.Exists(source + "-wal");
        bool sourceHadShm = File.Exists(source + "-shm");

        // Copy the database and any committed WAL/SHM so the snapshot sees the
        // same rows the running application sees.
        File.Copy(source, copyPath, overwrite: true);
        if (sourceHadWal)
            File.Copy(source + "-wal", copyPath + "-wal", overwrite: true);
        if (sourceHadShm)
            File.Copy(source + "-shm", copyPath + "-shm", overwrite: true);

        try
        {
            using (var connection = Open(copyPath))
                CheckpointWal(connection);

            DatabaseSnapshot before = SnapshotDatabase(copyPath, null);

            // Run the exact production startup path against the copy only.
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<AppDbContext>(
                options => options.UseSqlite(ConnectionString(copyPath)));

            using (var provider = services.BuildServiceProvider())
            {
                DbInitializer.InitializeAsync(provider).GetAwaiter().GetResult();
            }

            DatabaseSnapshot after = SnapshotDatabase(copyPath, before);

            var report = BuildReport(
                source, copyPath, sourceHadWal, before, after);

            string reportPath = Path.Combine(
                Environment.CurrentDirectory, "docs", "evidence", "migration-audit.json");

            Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
            File.WriteAllText(
                reportPath,
                JsonSerializer.Serialize(
                    report,
                    new JsonSerializerOptions { WriteIndented = true }));

            PrintSummary(report, reportPath);

            return report.Passed ? 0 : 1;
        }
        finally
        {
            // Pooled connections keep the file open on Windows; release them
            // before the working copy is removed.
            SqliteConnection.ClearAllPools();

            if (keep)
            {
                Console.WriteLine($"Working copy kept at: {copyPath}");
            }
            else
            {
                TryDeleteDirectory(work);
            }
        }
    }

    private static MigrationAuditReport BuildReport(
        string source,
        string copyPath,
        bool sourceHadWal,
        DatabaseSnapshot before,
        DatabaseSnapshot after)
    {
        var missingTables = before.Schema.Keys
            .Where(t => !after.Schema.ContainsKey(t))
            .OrderBy(t => t, StringComparer.Ordinal)
            .ToList();

        var missingColumns = new List<string>();

        foreach (var (table, columns) in before.Schema)
        {
            if (!after.Schema.TryGetValue(table, out var afterColumns))
                continue;

            var present = new HashSet<string>(afterColumns, StringComparer.OrdinalIgnoreCase);
            foreach (string column in columns)
            {
                if (!present.Contains(column))
                    missingColumns.Add($"{table}.{column}");
            }
        }

        missingColumns.Sort(StringComparer.Ordinal);

        long removed = 0, changed = 0, added = 0;
        var removedKeys = new List<string>();
        var changedDetails = new List<ChangedRow>();
        var addedKeys = new List<string>();

        foreach (var (table, beforeTable) in before.Data)
        {
            if (!after.Data.TryGetValue(table, out var afterTable))
                continue;

            foreach (var (key, beforeHash) in beforeTable.RowHashes)
            {
                if (!afterTable.RowHashes.TryGetValue(key, out var afterHash))
                {
                    removed++;
                    if (removedKeys.Count < MaxDetail)
                        removedKeys.Add($"{table}:{key}");
                }
                else if (!string.Equals(afterHash, beforeHash, StringComparison.Ordinal))
                {
                    changed++;
                    if (changedDetails.Count < MaxDetail)
                    {
                        var beforeValues = beforeTable.Rows[key];
                        afterTable.Rows.TryGetValue(key, out var afterValues);

                        var differing = beforeValues
                            .Where(kv => afterValues is null
                                || !afterValues.TryGetValue(kv.Key, out var value)
                                || !string.Equals(value, kv.Value, StringComparison.Ordinal))
                            .Select(kv => kv.Key)
                            .ToList();

                        changedDetails.Add(new ChangedRow(table, key, differing));
                    }
                }
            }

            foreach (string key in afterTable.RowHashes.Keys)
            {
                if (!beforeTable.RowHashes.ContainsKey(key))
                {
                    added++;
                    if (addedKeys.Count < MaxDetail)
                        addedKeys.Add($"{table}:{key}");
                }
            }
        }

        var migrationsRemoved = before.Migrations
            .Except(after.Migrations, StringComparer.Ordinal)
            .OrderBy(m => m, StringComparer.Ordinal)
            .ToList();

        Dictionary<string, long> legacyUsage;
        using (var connection = Open(copyPath))
            legacyUsage = LegacyColumnUsage(connection);

        bool passed =
            string.Equals(after.Integrity, "ok", StringComparison.OrdinalIgnoreCase)
            && missingTables.Count == 0
            && missingColumns.Count == 0
            && removed == 0
            && migrationsRemoved.Count == 0
            && after.ForeignKeyViolations <= before.ForeignKeyViolations;

        return new MigrationAuditReport(
            DateTimeOffset.Now,
            source,
            new FileInfo(source).Length,
            sourceHadWal,
            copyPath,
            before.Integrity,
            after.Integrity,
            before.ForeignKeyViolations,
            after.ForeignKeyViolations,
            before.JournalMode,
            after.JournalMode,
            before.Schema.Count,
            after.Schema.Count,
            before.Data.ToDictionary(kv => kv.Key, kv => kv.Value.RowCount),
            after.Data.ToDictionary(kv => kv.Key, kv => kv.Value.RowCount),
            removed,
            changed,
            added,
            missingTables,
            missingColumns,
            removedKeys,
            changedDetails,
            addedKeys,
            before.Migrations,
            after.Migrations,
            migrationsRemoved,
            legacyUsage,
            passed);
    }

    private static void PrintSummary(MigrationAuditReport report, string reportPath)
    {
        Console.WriteLine($"Migration audit: {(report.Passed ? "PASSED" : "FAILED")}");
        Console.WriteLine($"Source: {report.SourceDatabase} ({report.SourceBytes} bytes, wal={report.SourceHadWal})");
        Console.WriteLine($"Integrity: {report.IntegrityBefore} -> {report.IntegrityAfter} | FK violations: {report.ForeignKeyViolationsBefore} -> {report.ForeignKeyViolationsAfter}");
        Console.WriteLine($"Journal: {report.JournalModeBefore} -> {report.JournalModeAfter}");
        Console.WriteLine($"Tables: {report.TableCountBefore} -> {report.TableCountAfter}");
        Console.WriteLine($"Rows removed: {report.RowsRemoved} | changed: {report.RowsChanged} | added: {report.RowsAdded}");
        Console.WriteLine($"Missing tables: {report.MissingTables.Count} | missing columns: {report.MissingColumns.Count}");
        Console.WriteLine($"Migrations: {report.MigrationsBefore.Count} before, {report.MigrationsAfter.Count} after, {report.MigrationsRemoved.Count} removed");
        Console.WriteLine("Legacy column usage:");

        foreach (var (key, value) in report.LegacyColumnUsage)
            Console.WriteLine($"  - {key}: {value}");

        Console.WriteLine($"Report: {reportPath}");
    }

    private static Dictionary<string, long> LegacyColumnUsage(SqliteConnection connection)
    {
        var result = new Dictionary<string, long>();

        if (TableExists(connection, "Products"))
        {
            result["Products.Total"] = ScalarLong(connection, "SELECT COUNT(*) FROM \"Products\";");

            if (ColumnExists(connection, "Products", "Barcode"))
            {
                result["Products.Barcode (non-empty)"] = ScalarLong(
                    connection,
                    "SELECT COUNT(*) FROM \"Products\" WHERE \"Barcode\" IS NOT NULL AND TRIM(\"Barcode\") <> '';");
            }

            if (ColumnExists(connection, "Products", "CostPriceAmount"))
            {
                result["Products.CostPriceAmount (non-zero)"] = ScalarLong(
                    connection,
                    "SELECT COUNT(*) FROM \"Products\" WHERE \"CostPriceAmount\" IS NOT NULL AND \"CostPriceAmount\" <> 0;");
            }

            if (ColumnExists(connection, "Products", "CostPriceCurrency"))
            {
                result["Products.CostPriceCurrency (set)"] = ScalarLong(
                    connection,
                    "SELECT COUNT(*) FROM \"Products\" WHERE \"CostPriceCurrency\" IS NOT NULL AND TRIM(\"CostPriceCurrency\") <> '';");
            }
        }

        if (TableExists(connection, "ProductVariants"))
        {
            result["ProductVariants.Total"] = ScalarLong(connection, "SELECT COUNT(*) FROM \"ProductVariants\";");

            if (ColumnExists(connection, "ProductVariants", "Name"))
            {
                result["ProductVariants.Name (non-empty)"] = ScalarLong(
                    connection,
                    "SELECT COUNT(*) FROM \"ProductVariants\" WHERE \"Name\" IS NOT NULL AND TRIM(\"Name\") <> '';");
            }

            if (ColumnExists(connection, "ProductVariants", "AdditionalPrice"))
            {
                result["ProductVariants.AdditionalPrice (non-zero)"] = ScalarLong(
                    connection,
                    "SELECT COUNT(*) FROM \"ProductVariants\" WHERE \"AdditionalPrice\" IS NOT NULL AND \"AdditionalPrice\" <> 0;");
            }
        }

        if (TableExists(connection, "ProductBarcodes"))
            result["ProductBarcodes.Total"] = ScalarLong(connection, "SELECT COUNT(*) FROM \"ProductBarcodes\";");

        if (TableExists(connection, "Products")
            && TableExists(connection, "ProductVariants")
            && ColumnExists(connection, "Products", "CostPriceAmount")
            && ColumnExists(connection, "ProductVariants", "CostPriceAmount"))
        {
            result["Products whose default variant cost differs from Product.CostPrice"] = ScalarLong(
                connection,
                "SELECT COUNT(*) FROM \"Products\" p "
                + "JOIN \"ProductVariants\" v ON v.\"ProductId\" = p.\"Id\" AND v.\"IsDefault\" = 1 "
                + "WHERE ABS(COALESCE(p.\"CostPriceAmount\", 0) - COALESCE(v.\"CostPriceAmount\", 0)) > 0.0001;");
        }

        return result;
    }

    // ── Snapshots ──────────────────────────────────────────────────────────

    private static DatabaseSnapshot SnapshotDatabase(string path, DatabaseSnapshot? columnSource)
    {
        using var connection = Open(path);

        var snapshot = new DatabaseSnapshot
        {
            Integrity = ScalarString(connection, "PRAGMA integrity_check;"),
            ForeignKeyViolations = CountRows(connection, "PRAGMA foreign_key_check;"),
            JournalMode = ScalarString(connection, "PRAGMA journal_mode;"),
            Tables = QueryStrings(
                connection,
                "SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%' ORDER BY name;")
        };

        foreach (string table in snapshot.Tables)
            snapshot.Schema[table] = GetColumns(connection, table).Select(c => c.Name).ToList();

        foreach (string table in snapshot.Tables)
        {
            var afterColumns = GetColumns(connection, table);

            List<ColumnInfo> useColumns;

            if (columnSource is null || !columnSource.Schema.TryGetValue(table, out var beforeNames))
            {
                useColumns = afterColumns;
            }
            else
            {
                // Re-hash using the columns that existed before the migration so
                // an added column can never look like a changed row.
                var map = afterColumns.ToDictionary(
                    c => c.Name, c => c, StringComparer.OrdinalIgnoreCase);

                useColumns = beforeNames
                    .Where(map.ContainsKey)
                    .Select(name => map[name])
                    .ToList();
            }

            snapshot.Data[table] = SnapshotTable(connection, table, useColumns);
        }

        if (snapshot.Schema.ContainsKey("__EFMigrationsHistory"))
        {
            snapshot.Migrations = QueryStrings(
                connection,
                "SELECT \"MigrationId\" FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\";");
        }

        return snapshot;
    }

    private static TableData SnapshotTable(
        SqliteConnection connection,
        string table,
        List<ColumnInfo> columns)
    {
        var data = new TableData
        {
            Columns = columns.Select(c => c.Name).ToList()
        };

        var pk = columns
            .Where(c => c.PrimaryKeyOrdinal > 0)
            .OrderBy(c => c.PrimaryKeyOrdinal)
            .Select(c => c.Name)
            .Where(data.Columns.Contains)
            .ToList();

        bool useRowId = pk.Count == 0;
        string columnList = string.Join(", ", data.Columns.Select(Quote));

        string sql = data.Columns.Count == 0
            ? $"SELECT rowid FROM {Quote(table)};"
            : useRowId
                ? $"SELECT rowid, {columnList} FROM {Quote(table)};"
                : $"SELECT {columnList} FROM {Quote(table)};";

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        using var reader = command.ExecuteReader();

        long count = 0;

        while (reader.Read())
        {
            count++;

            var values = new Dictionary<string, string>(StringComparer.Ordinal);

            for (int i = 0; i < data.Columns.Count; i++)
                values[data.Columns[i]] = Format(reader, i + (useRowId ? 1 : 0));

            long rowId = useRowId ? reader.GetInt64(0) : 0;
            string key = RowKey(pk, values, rowId);

            data.Rows[key] = values;
            data.RowHashes[key] = HashRow(data.Columns, values);
        }

        data.RowCount = count;
        return data;
    }

    private static string RowKey(
        IReadOnlyList<string> pkColumns,
        IReadOnlyDictionary<string, string> values,
        long rowId)
    {
        if (pkColumns.Count == 0)
            return "rowid:" + rowId.ToString(CultureInfo.InvariantCulture);

        var builder = new StringBuilder();

        foreach (string column in pkColumns)
        {
            builder.Append(values.TryGetValue(column, out var value) ? value : string.Empty);
            builder.Append('\u0001');
        }

        return builder.ToString();
    }

    private static string HashRow(
        IReadOnlyList<string> columns,
        IReadOnlyDictionary<string, string> values)
    {
        var builder = new StringBuilder();

        foreach (string column in columns)
        {
            builder.Append(column).Append('=');
            builder.Append(values.TryGetValue(column, out var value) ? value : string.Empty);
            builder.Append('\u001f');
        }

        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString())));
    }

    private static string Format(SqliteDataReader reader, int index)
    {
        if (reader.IsDBNull(index))
            return "\u0000NULL";

        object value = reader.GetValue(index);

        return value switch
        {
            byte[] bytes => "blob:" + Convert.ToHexString(bytes),
            double d => "real:" + d.ToString("R", CultureInfo.InvariantCulture),
            float f => "real:" + f.ToString("R", CultureInfo.InvariantCulture),
            decimal m => "dec:" + m.ToString(CultureInfo.InvariantCulture),
            long l => "int:" + l.ToString(CultureInfo.InvariantCulture),
            _ => "str:" + Convert.ToString(value, CultureInfo.InvariantCulture)
        };
    }

    // ── Raw SQLite helpers ─────────────────────────────────────────────────

    private static SqliteConnection Open(string path)
    {
        var connection = new SqliteConnection(ConnectionString(path));
        connection.Open();
        return connection;
    }

    private static string ConnectionString(string path) =>
        new SqliteConnectionStringBuilder { DataSource = path }.ToString();

    private static void CheckpointWal(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA wal_checkpoint(TRUNCATE);";
        command.ExecuteNonQuery();
    }

    private static List<ColumnInfo> GetColumns(SqliteConnection connection, string table)
    {
        var columns = new List<ColumnInfo>();

        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info({Quote(table)});";

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            string name = reader.GetString(1);
            string type = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
            int pk = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);

            columns.Add(new ColumnInfo(name, type, pk));
        }

        return columns;
    }

    private static string ScalarString(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return command.ExecuteScalar()?.ToString() ?? string.Empty;
    }

    private static long ScalarLong(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToInt64(command.ExecuteScalar() ?? 0L, CultureInfo.InvariantCulture);
    }

    private static int CountRows(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        using var reader = command.ExecuteReader();

        int count = 0;
        while (reader.Read())
            count++;

        return count;
    }

    private static List<string> QueryStrings(SqliteConnection connection, string sql)
    {
        var values = new List<string>();

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        using var reader = command.ExecuteReader();

        while (reader.Read())
            values.Add(reader.IsDBNull(0) ? string.Empty : reader.GetString(0));

        return values;
    }

    private static bool TableExists(SqliteConnection connection, string table) =>
        ScalarLong(
            connection,
            $"SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = '{table.Replace("'", "''")}';") > 0;

    private static bool ColumnExists(SqliteConnection connection, string table, string column)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info({Quote(table)});";

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            if (string.Equals(reader.GetString(1), column, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static string Quote(string identifier) =>
        "\"" + identifier.Replace("\"", "\"\"") + "\"";

    private static string? GetOption(string[] args, string name)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        }

        return null;
    }

    private static void TryDeleteDirectory(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    // ── Types ──────────────────────────────────────────────────────────────

    private sealed record ColumnInfo(string Name, string DeclaredType, int PrimaryKeyOrdinal);

    private sealed class TableData
    {
        public List<string> Columns { get; set; } = new();

        public long RowCount { get; set; }

        public Dictionary<string, Dictionary<string, string>> Rows { get; } =
            new(StringComparer.Ordinal);

        public Dictionary<string, string> RowHashes { get; } =
            new(StringComparer.Ordinal);
    }

    private sealed class DatabaseSnapshot
    {
        public string Integrity { get; set; } = string.Empty;

        public int ForeignKeyViolations { get; set; }

        public string JournalMode { get; set; } = string.Empty;

        public List<string> Tables { get; set; } = new();

        public Dictionary<string, List<string>> Schema { get; } =
            new(StringComparer.Ordinal);

        public Dictionary<string, TableData> Data { get; } =
            new(StringComparer.Ordinal);

        public List<string> Migrations { get; set; } = new();
    }

    private sealed record ChangedRow(
        string Table,
        string Key,
        IReadOnlyList<string> ChangedColumns);

    private sealed record MigrationAuditReport(
        DateTimeOffset GeneratedAt,
        string SourceDatabase,
        long SourceBytes,
        bool SourceHadWal,
        string WorkDatabase,
        string IntegrityBefore,
        string IntegrityAfter,
        int ForeignKeyViolationsBefore,
        int ForeignKeyViolationsAfter,
        string JournalModeBefore,
        string JournalModeAfter,
        int TableCountBefore,
        int TableCountAfter,
        IReadOnlyDictionary<string, long> RowCountsBefore,
        IReadOnlyDictionary<string, long> RowCountsAfter,
        long RowsRemoved,
        long RowsChanged,
        long RowsAdded,
        IReadOnlyList<string> MissingTables,
        IReadOnlyList<string> MissingColumns,
        IReadOnlyList<string> RemovedRowKeys,
        IReadOnlyList<ChangedRow> ChangedRowDetails,
        IReadOnlyList<string> AddedRowKeys,
        IReadOnlyList<string> MigrationsBefore,
        IReadOnlyList<string> MigrationsAfter,
        IReadOnlyList<string> MigrationsRemoved,
        IReadOnlyDictionary<string, long> LegacyColumnUsage,
        bool Passed);
}
