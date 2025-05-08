FROM hub.nexdev.net/base/dotnet/core/sdk:7.0 AS builder
WORKDIR /src

COPY src/WebAPI/MemberAPIs/MemberAPIs.csproj ./WebAPI/MemberAPIs/MemberAPIs.csproj
RUN dotnet nuget add source http://nuget.nexdev.net/api/v2
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore WebAPI/MemberAPIs/MemberAPIs.csproj -v n

COPY src/WebAPI/MemberAPIs ./WebAPI/MemberAPIs

WORKDIR /src/WebAPI/MemberAPIs
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish -r linux-x64 --self-contained false -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:7.0-alpine-25h1 as base

RUN adduser --disabled-password \
    --home /app \
    --gecos '' dotnetuser && chown -R dotnetuser /app && chown -R dotnetuser /tmp
USER dotnetuser

FROM base AS final
WORKDIR /app
COPY --from=builder /app .
ENV TZ="America/La_Paz"

ENTRYPOINT ["dotnet", "MemberAPIs.dll"]
