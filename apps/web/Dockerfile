# web (frontend). Byg fra RODEN af repoet, fordi den bruger ChatService.Contracts fra chat-microservice:
#   docker build -f apps/web/Dockerfile -t bizcord/web .
# Kør (gatewayens adresse som miljøvariabel):
#   docker run -p 5186:8080 -e GatewayApi__BaseUrl="http://<gateway>:8080/" bizcord/web

# ---- Trin 1: byg med SDK'et ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo

# Projektfilerne først: så genbruger Docker restore-laget, så længe pakkerne ikke ændres
COPY apps/web/src/Bizcord/Bizcord.csproj apps/web/src/Bizcord/
COPY apps/chat-microservice/src/ChatService.Contracts/ChatService.Contracts.csproj apps/chat-microservice/src/ChatService.Contracts/
RUN dotnet restore apps/web/src/Bizcord/Bizcord.csproj

# Kun frontendens kode og kontrakterne (ikke resten af chat-microservice)
COPY apps/web/src/ apps/web/src/
COPY apps/chat-microservice/src/ChatService.Contracts/ apps/chat-microservice/src/ChatService.Contracts/
RUN dotnet publish apps/web/src/Bizcord/Bizcord.csproj -c Release -o /app --no-restore

# ---- Trin 2: kør med kun runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "Bizcord.dll"]
