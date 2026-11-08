# chat-microservice. Byg fra RODEN af repoet:
#   docker build -f apps/chat-microservice/Dockerfile -t bizcord/chat-microservice .
# Kør (connection string som miljøvariabel, ingen user-secrets i en container):
#   docker run -p 5210:8080 -e ConnectionStrings__ChatDb="Host=...;Database=chatdb;Username=...;Password=..." bizcord/chat-microservice

# ---- Trin 1: byg med SDK'et (stort image, bruges kun til at bygge) ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo

# Projektfilerne først: så genbruger Docker restore-laget, så længe pakkerne ikke ændres
COPY apps/chat-microservice/src/ChatService/ChatService.csproj apps/chat-microservice/src/ChatService/
COPY apps/chat-microservice/src/ChatService.Contracts/ChatService.Contracts.csproj apps/chat-microservice/src/ChatService.Contracts/
COPY apps/chat-microservice/src/MessageClient/MessageClient.csproj apps/chat-microservice/src/MessageClient/
RUN dotnet restore apps/chat-microservice/src/ChatService/ChatService.csproj

# Så resten af koden, og publish
COPY apps/chat-microservice/src/ apps/chat-microservice/src/
RUN dotnet publish apps/chat-microservice/src/ChatService/ChatService.csproj -c Release -o /app --no-restore

# ---- Trin 2: kør med kun runtime (lille image uden SDK og kildekode) ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# .NET-images lytter på 8080 og kører som en bruger uden admin-rettigheder
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "ChatService.dll"]
