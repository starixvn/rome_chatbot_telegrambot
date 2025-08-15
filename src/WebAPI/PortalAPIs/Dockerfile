FROM hub.nexdev.net/base/dotnet/core/sdk:6.0 AS builder
WORKDIR /src

COPY src/WebAPI/PortalAPIs/PortalAPIs.csproj ./WebAPI/PortalAPIs/PortalAPIs.csproj
RUN dotnet nuget add source http://nuget.nexdev.net/api/v2
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore WebAPI/PortalAPIs/PortalAPIs.csproj -v n

COPY src/Shared/TelegramBot.Utilities ./Shared/TelegramBot.Utilities
COPY src/WebAPI/PortalAPIs ./WebAPI/PortalAPIs

WORKDIR /src/WebAPI/PortalAPIs
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish -r linux-x64 --self-contained false -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:6.0-alpine-25h1 AS base

RUN adduser --disabled-password \
    --home /app \
    --gecos '' dotnetuser && chown -R dotnetuser /app && chown -R dotnetuser /tmp
USER dotnetuser

WORKDIR /app
COPY --from=builder /app .
ENV TZ="America/La_Paz"

ENTRYPOINT ["dotnet", "PortalAPIs.dll"]
