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
                new() { Name = "ريال سعودي", Code = "SAR", Symbol = "﷼", ExchangeRate = 1m, IsDefault = true },
                new() { Name = "درهم إماراتي", Code = "AED", Symbol = "د.إ", ExchangeRate = 0.98m, IsDefault = false },
                new() { Name = "دينار كويتي", Code = "KWD", Symbol = "د.ك", ExchangeRate = 8.12m, IsDefault = false },
                new() { Name = "دولار أمريكي", Code = "USD", Symbol = "$", ExchangeRate = 3.75m, IsDefault = false },
                new() { Name = "يورو", Code = "EUR", Symbol = "€", ExchangeRate = 4.08m, IsDefault = false },
                new() { Name = "جنيه مصري", Code = "EGP", Symbol = "ج.م", ExchangeRate = 0.12m, IsDefault = false },
            };

            await context.Currencies.AddRangeAsync(currencies);
            await context.SaveChangesAsync();
        }
    }
}
