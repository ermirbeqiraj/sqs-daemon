FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Daemon/Daemon.csproj", "Daemon/"]
COPY ["Infrastructure.HttpService/Infrastructure.HttpService.csproj", "Infrastructure.HttpService/"]
COPY ["Infrastructure.QueueService/Infrastructure.QueueService.csproj", "Infrastructure.QueueService/"]
RUN dotnet restore "Daemon/Daemon.csproj"
COPY . .
WORKDIR "/src/Daemon"
RUN dotnet build "Daemon.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Daemon.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine-amd64 as final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Daemon.dll"]