FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar sólo archivos del proyecto web para optimizar cache
COPY BotCotizacionWeb/*.csproj ./BotCotizacionWeb/
RUN dotnet restore BotCotizacionWeb/BotCotizacionWeb.csproj

COPY BotCotizacionWeb/. ./BotCotizacionWeb/
WORKDIR /src/BotCotizacionWeb
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BotCotizacionWeb.dll"]
