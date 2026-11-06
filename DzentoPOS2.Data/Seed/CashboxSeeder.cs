using DzentoPOS2.Data.Context;

namespace DzentoPOS2.Data.Seed
{
    /// <summary>
    /// Kept for compatibility. Cashbox creation is now handled by the
    /// idempotent <see cref="StoreBootstrapper"/> so the branch, warehouse and
    /// cashbox always agree on a single main branch.
    /// </summary>
    public static class CashboxSeeder
    {
        public static Task SeedAsync(AppDbContext context)
            => StoreBootstrapper.EnsureInitializedAsync(context);
    }
}
