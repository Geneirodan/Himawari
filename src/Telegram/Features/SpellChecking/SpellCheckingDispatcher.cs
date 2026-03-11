using Himawari.SpellChecking.Models;
using Himawari.SpellChecking.Replies;
using Himawari.SpellChecking.Services;
using Himawari.Telegram.Core.Abstractions;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;

namespace Himawari.SpellChecking;

/// <summary>
/// Message handler: when <see cref="IWrongLayoutParser.TryParse"/> succeeds on the message text and <see cref="SpellCheckingOptions.SendWrongLayoutReply"/> is enabled, sends <see cref="SendCorrectedTextMessageReply"/> so the user receives the corrected text. When disabled, wrong-layout detection is not reported (layout correction for commands still applies via <see cref="ICommandLayoutCorrector"/>).
/// </summary>
[PublicAPI]
public sealed class SpellCheckingDispatcher(IWrongLayoutParser parser, IServiceProvider serviceProvider, IOptions<SpellCheckingOptions> options)
    : AbstractDispatcher
{
    /// <inheritdoc />
    protected override async Task OnNewMessage(Message msg)
    {
        if (msg.Text is null || !options.Value.SendWrongLayoutReply || !parser.TryParse(msg.Text, out var correctedText))
            return;

        var message = new SendCorrectedTextMessageReply(msg, correctedText);
        var scope = serviceProvider.CreateAsyncScope();
        await using (scope.ConfigureAwait(false))
            await scope.ServiceProvider.GetRequiredService<ISender>().Send(message).ConfigureAwait(false);
    }
}