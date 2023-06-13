FROM hub.nexdev.net/base/dotnet/core/sdk:6.0-alpine as builder
WORKDIR /src

COPY src/Scheduler/TelegramBot.ImporterService/TelegramBot.ImporterService.csproj ./TelegramBot.ImporterService/
RUN dotnet nuget add source http://nuget.nexdev.net/api/v2
RUN dotnet restore TelegramBot.ImporterService/TelegramBot.ImporterService.csproj -v n

COPY src/Scheduler/TelegramBot.ImporterService ./TelegramBot.ImporterService
COPY src/Scheduler/TelegramBot.Importer ./TelegramBot.Importer

WORKDIR /src/TelegramBot.ImporterService
RUN dotnet publish -r linux-x64 --self-contained false -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:6.0-alpine as base

WORKDIR /app
COPY --from=builder /app .
ENV TZ="America/La_Paz"

ENTRYPOINT ["dotnet", "TelegramBot.ImporterService.dll"]
