#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Server/ScrumPokerTool.Server.csproj", "src/Server/"]
COPY ["src/Client/ScrumPokerTool.Client.csproj", "src/Client/"]
COPY ["src/ScrumPokerTool.Shared/ScrumPokerTool.Shared.csproj", "src/ScrumPokerTool.Shared/"]
RUN dotnet restore "src/Server/ScrumPokerTool.Server.csproj"
COPY . .
WORKDIR "/src/src/Server"
RUN dotnet build "ScrumPokerTool.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ScrumPokerTool.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ScrumPokerTool.Server.dll"]