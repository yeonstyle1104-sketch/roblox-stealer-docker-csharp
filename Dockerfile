FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RobloxStealer.csproj", "."]
RUN dotnet restore "RobloxStealer.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "RobloxStealer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RobloxStealer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 10000
ENTRYPOINT ["dotnet", "RobloxStealer.dll"]
