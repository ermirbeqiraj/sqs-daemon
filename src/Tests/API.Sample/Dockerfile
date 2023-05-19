FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Tests/API.Sample/API.Sample.csproj", "Tests/API.Sample/"]
RUN dotnet restore "Tests/API.Sample/API.Sample.csproj"
COPY . .
WORKDIR "/src/Tests/API.Sample"
RUN dotnet build "API.Sample.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "API.Sample.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine-amd64 as final
EXPOSE 5050
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.Sample.dll"]