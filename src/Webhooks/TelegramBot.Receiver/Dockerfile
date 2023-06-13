FROM hub.nexdev.net/base/dotnet/core/sdk:6.0-alpine as builder
WORKDIR /src

COPY src/Webhooks/TelegramBot.Receiver/TelegramBot.Receiver.csproj ./Webhooks/TelegramBot.Receiver/TelegramBot.Receiver.csproj
RUN dotnet nuget add source http://nuget.nexdev.net/api/v2
RUN dotnet restore Webhooks/TelegramBot.Receiver/TelegramBot.Receiver.csproj -v n

COPY src/Shared/TelegramBot.Utilities ./Shared/TelegramBot.Utilities
COPY src/WebAPI/TelegramBot.Client ./WebAPI/TelegramBot.Client
COPY src/Webhooks/TelegramBot.Receiver ./Webhooks/TelegramBot.Receiver

WORKDIR /src/Webhooks/TelegramBot.Receiver
RUN dotnet publish -r linux-x64 --self-contained false -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:6.0-alpine as base

RUN adduser --disabled-password \
    --home /app \
    --gecos '' dotnetuser && chown -R dotnetuser /app
USER dotnetuser

WORKDIR /app
COPY --from=builder /app .
ENV TZ="America/La_Paz"

ENTRYPOINT ["dotnet", "TelegramBot.Receiver.dll"]
