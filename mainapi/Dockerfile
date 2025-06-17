# Используйте более легкий образ для финального этапа
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["mainapi/LunkvayAPI.csproj", "mainapi/"]
RUN dotnet restore "mainapi/LunkvayAPI.csproj"
COPY . .
WORKDIR "/src/mainapi"
RUN dotnet build "LunkvayAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LunkvayAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LunkvayAPI.dll"]