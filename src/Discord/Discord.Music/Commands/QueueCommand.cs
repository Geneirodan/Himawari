using System.Text;
using Ardalis.Result;
using DisCatSharp.ApplicationCommands.Context;
using Geneirodan.MediatR.Abstractions;
using Himawari.Discord.Music.Abstractions;
using Himawari.Discord.Music.Extensions;
using MediatR;

namespace Himawari.Discord.Music.Commands;

public sealed record QueueCommand(BaseContext Context) : PlayerCommandBase(Context), ICommand<string>
{
    public sealed class Handler : IRequestHandler<QueueCommand, Result<string>>
    {
        public Task<Result<string>> Handle(QueueCommand request, CancellationToken cancellationToken) =>
            Task.FromResult<Result<string>>(
                request.Player.Queue.Aggregate(
                    seed: new StringBuilder("Queued tracks:"),
                    func: (sb, track) => sb.AppendLine().Append(track.CreateTrackName()).Append(';'),
                    resultSelector: sb => sb.ToString()
                )
            );
    }
}