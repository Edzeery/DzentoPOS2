using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Seed
{
    public static class CurrencySeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Currencies.AnyAsync())
                return;

            var currencies = new List<Currency>
            {
                new() { Name = AppStrings.CurrencySaudiRiyal, Code = "SAR", Symbol = AppStrings.CurrencySymbolRialSign, ExchangeRate = 1m, IsDefault = true },
                new() { Name = AppStrings.CurrencyUaeDirham, Code = "AED", Symbol = AppStrings.CurrencySymbolAed, ExchangeRate = 0.98m, IsDefault = false },
                new() { Name = AppStrings.CurrencyKuwaitiDinar, Code = "KWD", Symbol = AppStrings.CurrencySymbolKwd, ExchangeRate = 8.12m, IsDefault = false },
                new() { Name = AppStrings.CurrencyUsDollar, Code = "USD", Symbol = "$", ExchangeRate = 3.75m, IsDefault = false },
                new() { Name = AppStrings.CurrencyEuro, Code = "EUR", Symbol = "€", ExchangeRate = 4.08m, IsDefault = false },
                new() { Name = AppStrings.CurrencyEgyptianPound, Code = "EGP", Symbol = AppStrings.CurrencySymbolEgp, ExchangeRate = 0.12m, IsDefault = false },
            };

            await context.Currencies.AddRangeAsync(currencies);
            await context.SaveChangesAsync();
        }
    }
}
