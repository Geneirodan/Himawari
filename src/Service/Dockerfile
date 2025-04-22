# syntax=docker/dockerfile:1.15.0
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
RUN apt-get update && apt-get install ffmpeg
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "src/"]
COPY ["Directory.Packages.props", "src/"]
COPY ["Service/Service.csproj", "src/Service/"]
COPY ["Application/Application.csproj", "src/Application/"]
COPY ["Core/Core.csproj", "src/Core/"]
COPY ["Features/SpellChecking/SpellChecking.csproj", "src/Features/SpellChecking/"]
COPY ["Games/Alias/Alias.csproj", "src/Games/Alias/"]
RUN dotnet nuget remove source Github
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
