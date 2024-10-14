using System.Diagnostics.Eventing.Reader;
using System.Reflection;
using DeepL;
using Himawari.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using TL;
using WeCantSpell.Hunspell;

namespace Himawari.SpellChecking;

public static class DependencyInjection
{
    private static string? dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    private static string dictDir = Path.Combine(dir ?? string.Empty, "Dictionaries");

    public static IServiceCollection AddSpellChecking(this IServiceCollection services) =>
        services
            .AddMediatR(x=>x.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()))
            .AddKeyedSingleton(LanguageCode.English, ImplementationFactory)
            .AddKeyedSingleton(LanguageCode.Ukrainian, ImplementationFactory)
            //.AddKeyedSingleton(LanguageCode.Russian, ImplementationFactory)
            .AddSingleton<IKeyboardLayoutService, KeyboardLayoutService>()
            .AllowResolvingKeyedServicesAsDictionary();

    private static WordList ImplementationFactory(IServiceProvider _, object o)
    {
        return WordList.CreateFromFiles($"{dictDir}\\{o}.dic", $"{dictDir}\\{o}.aff");
    }
}