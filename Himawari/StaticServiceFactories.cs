using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using WTelegram;

namespace Himawari;

public static class StaticServiceFactories
{
    public static SqliteConnection DbConnectionFactory(IServiceProvider serviceProvider)
    {
        var connectionString = serviceProvider
            .GetRequiredService<IConfiguration>()
            .GetConnectionString("DefaultConnection");
        return new SqliteConnection(connectionString);
    }

    public static Bot BotFactory(IServiceProvider serviceProvider)
    {
        var connection = serviceProvider.GetRequiredService<SqliteConnection>();
        var botOptions = serviceProvider.GetRequiredService<IOptions<BotOptions>>().Value;
        return new Bot(botOptions.Token, botOptions.ApiId, botOptions.ApiHash, connection);
    }
}