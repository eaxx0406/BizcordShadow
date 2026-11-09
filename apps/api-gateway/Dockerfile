# api-gateway. Byg fra RODEN af repoet:
#   docker build -f apps/api-gateway/Dockerfile -t bizcord/api-gateway .
# NB: routes i Ocelot.json peger på localhost:5210. I Docker skal de pege på servicenavne (kommer med compose).

# ---- Trin 1: byg med SDK'et ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /repo

# Projektfilen først: så genbruger Docker restore-laget, så længe pakkerne ikke ændres
COPY apps/api-gateway/src/GatewayAPI/GatewayAPI.csproj apps/api-gateway/src/GatewayAPI/
RUN dotnet restore apps/api-gateway/src/GatewayAPI/GatewayAPI.csproj

COPY apps/api-gateway/src/ apps/api-gateway/src/
RUN dotnet publish apps/api-gateway/src/GatewayAPI/GatewayAPI.csproj \
    -c $BUILD_CONFIGURATION -o /app --no-restore /p:UseAppHost=false

# ---- Trin 2: kør med kun runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "GatewayAPI.dll"]
