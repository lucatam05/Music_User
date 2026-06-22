FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG NUGET_USERNAME
ARG NUGET_PASSWORD
WORKDIR /src
COPY . .
RUN dotnet nuget remove source lucatam05 || true
RUN dotnet nuget add source "https://nuget.pkg.github.com/lucatam05/index.json" --name lucatam05 --username $NUGET_USERNAME --password $NUGET_PASSWORD --store-password-in-clear-text
RUN dotnet restore "Music.User.WebApi/Music.User.WebApi.csproj"
WORKDIR "/src/Music.User.WebApi"
RUN dotnet build "./Music.User.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Music.User.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Music.User.WebApi.dll"]