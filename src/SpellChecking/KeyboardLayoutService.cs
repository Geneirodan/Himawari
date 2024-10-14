using System.Text.RegularExpressions;
using DeepL;
using Himawari.Core.Abstractions;
using Himawari.Core.Extensions;
using Himawari.SpellChecking.Extensions;
using Himawari.SpellChecking.Keyboards;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WeCantSpell.Hunspell;

namespace Himawari.SpellChecking;

public interface IKeyboardLayoutService : IMessageHandler;

public partial class KeyboardLayoutService(
    KeyedServiceExtensions.IKeyedServiceDictionary<WordList> wordLists,
    IServiceProvider serviceProvider
) : IKeyboardLayoutService
{
    private const float Threshold = 0.5f;

    private string? OrfFilter(string inputString)
    {
        foreach (var (lang, wordList) in wordLists.Where(x => x.Key != LanguageCode.English))
        {
            var keyboard = Keyboard.GetKeyboard(lang);

            var words = WhitespaceRegex().Split(inputString).Where(x => x.Length > 0).ToList();

            double newHits = words
                .Select(word => word.Translate(keyboard))
                .SelectMany(newString => AllowedCharactersRegex().Matches(newString))
                .Select(x => x.Value)
                .Count(newWordToCheck => wordList.Check(newWordToCheck));

            if (newHits / words.Count > Threshold)
                return inputString.Translate(keyboard);

            double reversedHits = words
                .Select(word => word.TranslateReversed(keyboard))
                .Select(reversedWord => AllowedCharactersRegex().Matches(reversedWord).First().Value)
                .Count(reversedWordToCheck => wordList.Check(reversedWordToCheck));

            if (reversedHits / words.Count > Threshold)
                return inputString.TranslateReversed(keyboard);
        }

        return null;
    }

    public async Task OnMessage(Message msg, UpdateType update)
    {
        if (msg.Text is not { } messageText)
            return;


        if (OrfFilter(messageText) is { } correctedText)
        {
            var message = new SendCorrectedTextMessage(msg, correctedText);
            using var scope = serviceProvider.CreateScope();
            await scope.ServiceProvider.GetRequiredService<ISender>().Send(message);
        }
    }

    [GeneratedRegex(@"[\s]")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"['\-\w]+")]
    private static partial Regex AllowedCharactersRegex();
}