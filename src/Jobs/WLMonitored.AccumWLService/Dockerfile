FROM hub.nexdev.net/base/dotnet/core/sdk:6.0-alpine as builder
WORKDIR /src

COPY src/Jobs/WLMonitored.AccumWLService/WLMonitored.AccumWLService.csproj ./WLMonitored.AccumWLService/WLMonitored.AccumWLService.csproj
RUN dotnet nuget add source http://nuget.nexdev.net/api/v2
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore WLMonitored.AccumWLService/WLMonitored.AccumWLService.csproj -v n

COPY src/Jobs/WLMonitored.Shared ./WLMonitored.Shared
COPY src/Jobs/WLMonitored.AccumWLService ./WLMonitored.AccumWLService

WORKDIR /src/WLMonitored.AccumWLService
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish -r linux-x64 --self-contained false -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:6.0-alpine-25h1 as base

RUN adduser --disabled-password \
    --home /app \
    --gecos '' dotnetuser && chown -R dotnetuser /app && chown -R dotnetuser /tmp
USER dotnetuser

WORKDIR /app
COPY --from=builder /app .
ENV TZ="America/La_Paz"

ENTRYPOINT ["dotnet", "WLMonitored.AccumWLService.dll"]
