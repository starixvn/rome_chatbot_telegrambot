FROM hub.nexdev.net/base/dotnet/core/sdk:6.0-alpine as builder
WORKDIR /src

COPY src/Scheduler/TelegramBot.SenderService/TelegramBot.SenderService.csproj ./Scheduler/TelegramBot.SenderService/TelegramBot.SenderService.csproj
RUN dotnet nuget add source http://nuget.nexdev.net/api/v2
RUN dotnet restore Scheduler/TelegramBot.SenderService/TelegramBot.SenderService.csproj -v n

COPY src/Shared/TelegramBot.Utilities ./Shared/TelegramBot.Utilities
COPY src/Scheduler/TelegramBot.SenderService ./Scheduler/TelegramBot.SenderService

WORKDIR /src/Scheduler/TelegramBot.SenderService
RUN dotnet publish -r linux-x64 --self-contained false -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:6.0-alpine as base

RUN adduser --disabled-password \
    --home /app \
    --gecos '' dotnetuser && chown -R dotnetuser /app
USER dotnetuser

WORKDIR /app
COPY --from=builder /app .
ENV TZ="America/La_Paz"

ENTRYPOINT ["dotnet", "TelegramBot.SenderService.dll"]
