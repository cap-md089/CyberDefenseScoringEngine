#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ScoringEngine.Server/ScoringEngine.Server.csproj", "ScoringEngine.Server/"]
RUN dotnet restore "ScoringEngine.Server/ScoringEngine.Server.csproj"
COPY . .
WORKDIR "/src/ScoringEngine.Server"
RUN dotnet build "ScoringEngine.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ScoringEngine.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ScoringEngine.Server.dll"]