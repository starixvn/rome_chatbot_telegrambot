FROM hub.nexdev.net/base/dotnet/core/sdk:6.0-alpine as builder
WORKDIR /src

COPY src/WebAPI/TelegramBot.WebApi/TelegramBot.WebApi.csproj ./WebAPI/TelegramBot.WebApi/TelegramBot.WebApi.csproj
RUN dotnet nuget add source http://nuget.nexdev.net/api/v2
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore WebAPI/TelegramBot.WebApi/TelegramBot.WebApi.csproj -v n

COPY src/Shared/TelegramBot.Utilities ./Shared/TelegramBot.Utilities
COPY src/WebAPI/TelegramBot ./WebAPI/TelegramBot
COPY src/WebAPI/TelegramBot.WebApi ./WebAPI/TelegramBot.WebApi

WORKDIR /src/WebAPI/TelegramBot.WebApi
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish -r linux-x64 --self-contained false -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:6.0-alpine-25h1 as base

# Don’t run as root
RUN adduser --disabled-password \
    --home /app \
    --gecos '' dotnetuser && chown -R dotnetuser /app && chown -R dotnetuser /tmp
USER dotnetuser
#

WORKDIR /app
COPY --from=builder /app .
ENV TZ="America/La_Paz"

ENTRYPOINT ["dotnet", "TelegramBot.WebApi.dll"]
