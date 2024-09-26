using WTelegram;

namespace Himawari.Services;

public interface ICommandService
{
    CommandInfo? GetCommandByName(string command);
    void Configure(Bot bot);
}