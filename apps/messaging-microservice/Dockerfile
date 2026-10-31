FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY global.json Directory.Build.props NuGet.Config ./
COPY packages/MessageClient/src/Bizcord.MessageClient.csproj packages/MessageClient/src/
COPY packages/MessageClient/src/packages.lock.json packages/MessageClient/src/
COPY packages/Shared.Contracts/Bizcord.Shared.Contracts.csproj packages/Shared.Contracts/
COPY packages/Shared.Contracts/packages.lock.json packages/Shared.Contracts/
COPY apps/messaging-microservice/src/Bizcord.Messaging.Api/Bizcord.Messaging.Api.csproj apps/messaging-microservice/src/Bizcord.Messaging.Api/
COPY apps/messaging-microservice/src/Bizcord.Messaging.Api/packages.lock.json apps/messaging-microservice/src/Bizcord.Messaging.Api/
RUN dotnet restore apps/messaging-microservice/src/Bizcord.Messaging.Api/Bizcord.Messaging.Api.csproj --locked-mode
COPY packages/ packages/
COPY apps/messaging-microservice/src/ apps/messaging-microservice/src/
RUN dotnet publish apps/messaging-microservice/src/Bizcord.Messaging.Api/Bizcord.Messaging.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ConnectionStrings__Messages="Data Source=/app/data/messages.db"
COPY --from=build /app/publish .
RUN mkdir -p /app/data && chown app:app /app/data
USER app
ENTRYPOINT ["dotnet", "Bizcord.Messaging.Api.dll"]
