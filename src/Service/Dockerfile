# syntax=docker/dockerfile:1.15.0
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "src/"]
COPY ["Directory.Packages.props", "src/"]
COPY ["Service/Service.csproj", "src/Service/"]
COPY ["Telegram/Telegram.Application/Telegram.Application.csproj", "src/Telegram.Application/"]
COPY ["Telegram/Telegram.Core/Telegram.Core.csproj", "src/Telegram.Core/"]
COPY ["Common/Shared/Shared.csproj", "src/Common/Shared/"]
COPY ["Common/CobaltTools/CobaltTools.csproj", "src/Common/CobaltTools/"]
COPY ["Telegram/Features/SillyThings/SillyThings.csproj", "src/Telegram/Features/SillyThings/"]
COPY ["Telegram/Features/SpellChecking/SpellChecking.csproj", "src/Telegram/Features/SpellChecking/"]
COPY ["Telegram/Features/Todolist/Todolist.csproj", "src/Telegram/Features/Todolist/"]
COPY ["Telegram/Features/VideoParser/VideoParser.csproj", "src/Telegram/Features/VideoParser/"]
COPY ["Telegram/Games/Alias/Alias.csproj", "src/Telegram/Games/Alias/"] 
COPY ["Discord/Discord.Core/Discord.Core.csproj", "src/Discord/Discord.Core/"]
COPY ["Discord/Discord.Music/Discord.Music.csproj", "src/Discord/Discord.Music/"]
RUN --mount=type=secret,id=github-username \
    --mount=type=secret,id=github-token \
    dotnet nuget add source "https://nuget.pkg.github.com/Geneirodan/index.json" \
                --name Github \
                --username $(cat /run/secrets/github-username) \
                --password $(cat /run/secrets/github-token) \
                --store-password-in-clear-text
RUN dotnet restore "src/Service/Service.csproj"
COPY . .
WORKDIR "/src/Service"
RUN dotnet build "Service.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Service.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Service.dll"]
