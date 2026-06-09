
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PractoBackend.csproj", "./"]
RUN dotnet restore "PractoBackend.csproj"
COPY . .
RUN dotnet build "PractoBackend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PractoBackend.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PractoBackend.dll"]
