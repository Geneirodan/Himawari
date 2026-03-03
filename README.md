# Himawari

Telegram and Discord bots (single host). Geneirodan.Observability and Geneirodan.MediatR are referenced from GitHub Packages (`PackageReference`).

## Build

Restore and build require NuGet authentication to GitHub Packages (e.g. `nuget.config` with token or `NUGET_TOKEN`). From `src/Service`:

```bash
dotnet run
```

**Secrets (do not commit):** set via [user-secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) or environment.

- `Telegram:Bot:Token`, `Telegram:Bot:ApiId`, `Telegram:Bot:ApiHash` — from BotFather and my.telegram.org.
- `Discord:Token` — for Discord bot.
- `MEDIATR_KEY` — if required by Geneirodan.MediatR.