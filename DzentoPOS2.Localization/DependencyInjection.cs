using DzentoPOS2.Localization.Services;
using DzentoPOS2.Localization.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DzentoPOS2.Localization
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddLocalizationModule(
            this IServiceCollection services)
        {
            services.AddSingleton<ILanguageService, LanguageService>();

            return services;
        }
    }
}