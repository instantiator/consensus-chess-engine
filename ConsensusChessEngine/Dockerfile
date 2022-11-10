FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["ConsensusChessEngine.sln", "ConsensusChessEngine.sln"]
COPY ["ConsensusChessEngine/ConsensusChessEngine.csproj", "ConsensusChessEngine/"]
COPY ["ConsensusChessNode/ConsensusChessNode.csproj", "ConsensusChessNode/"]
COPY ["ConsensusChessShared/ConsensusChessShared.csproj", "ConsensusChessShared/"]
COPY ["ConsensusChessSharedTests/ConsensusChessSharedTests.csproj", "ConsensusChessSharedTests/"]

RUN dotnet restore

COPY . .

WORKDIR "/src"
RUN dotnet test

WORKDIR "/src/ConsensusChessEngine"
RUN dotnet build "ConsensusChessEngine.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConsensusChessEngine.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConsensusChessEngine.dll"]
