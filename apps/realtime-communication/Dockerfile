# realtime-communication (dummy-servicen, der modtager MessageSent). Byg fra RODEN af repoet:
#   docker build -f apps/realtime-communication/Dockerfile -t bizcord/realtime-communication .
# Kør (connection strings som miljøvariabler, ingen user-secrets i en container):
#   docker run -p 5220:8080 -e ConnectionStrings__RealTimeDb="..." -e ConnectionStrings__RabbitMQ="..." bizcord/realtime-communication

# ---- Trin 1: byg med SDK'et (stort image, bruges kun til at bygge) ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /repo

# Projektfilerne først: så genbruger Docker restore-laget, så længe pakkerne ikke ændres.
# Samme mapper som i repoet, så ProjectReference-stierne (..\..\..\..\shared\...) passer
COPY apps/realtime-communication/src/RealTimeCommunicationService/RealTimeCommunicationService.csproj apps/realtime-communication/src/RealTimeCommunicationService/
COPY apps/chat-microservice/src/ChatService.Contracts/ChatService.Contracts.csproj apps/chat-microservice/src/ChatService.Contracts/
COPY shared/src/MessageClient/MessageClient.csproj shared/src/MessageClient/
RUN dotnet restore apps/realtime-communication/src/RealTimeCommunicationService/RealTimeCommunicationService.csproj

# Så resten af koden: servicen selv + det, den afhænger af (ikke hele chat-microservice)
COPY apps/realtime-communication/src/ apps/realtime-communication/src/
COPY apps/chat-microservice/src/ChatService.Contracts/ apps/chat-microservice/src/ChatService.Contracts/
COPY shared/src/ shared/src/
RUN dotnet publish apps/realtime-communication/src/RealTimeCommunicationService/RealTimeCommunicationService.csproj \
    -c $BUILD_CONFIGURATION -o /app --no-restore /p:UseAppHost=false

# ---- Trin 2: kør med kun runtime (lille image uden SDK og kildekode) ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# .NET-images lytter på 8080 og kører som en bruger uden admin-rettigheder
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "RealTimeCommunicationService.dll"]
