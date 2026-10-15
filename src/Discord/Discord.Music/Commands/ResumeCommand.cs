using Ardalis.Result;
using DisCatSharp.ApplicationCommands.Context;
using Himawari.Discord.Music.Abstractions;
using Himawari.Discord.Music.Extensions;
using Mediator;

namespace Himawari.Discord.Music.Commands;

public sealed record ResumeCommand(BaseContext Context) : CurrentTrackCommandBase(Context), ICommand<Result<string>>
{
    public sealed class Handler : ICommandHandler<ResumeCommand, Result<string>>
    {
        public async ValueTask<Result<string>> Handle(ResumeCommand request, CancellationToken cancellationToken)
        {
            await request.Player.ResumeAsync().ConfigureAwait(false);
            return request.CurrentTrack.CreateTrackName();
        }
    }
}
