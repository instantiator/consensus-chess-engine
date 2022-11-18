FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["ConsensusChessEngine/ConsensusChessEngine.csproj", "ConsensusChessEngine/"]
COPY ["ConsensusChessNode/ConsensusChessNode.csproj", "ConsensusChessNode/"]
COPY ["ConsensusChessShared/ConsensusChessShared.csproj", "ConsensusChessShared/"]
COPY ["ConsensusChessSharedTests/ConsensusChessSharedTests.csproj", "ConsensusChessSharedTests/"]
COPY ["ConsensusChessFeatureTests/ConsensusChessFeatureTests.csproj", "ConsensusChessFeatureTests/"]

RUN dotnet restore ConsensusChessNode/ConsensusChessNode.csproj
RUN dotnet restore ConsensusChessSharedTests/ConsensusChessSharedTests.csproj
RUN dotnet restore ConsensusChessFeatureTests/ConsensusChessFeatureTests.csproj

COPY . .

WORKDIR "/src"
RUN dotnet test ConsensusChessSharedTests/ConsensusChessSharedTests.csproj
RUN dotnet test ConsensusChessFeatureTests/ConsensusChessFeatureTests.csproj

WORKDIR "/src/ConsensusChessNode"
RUN dotnet build "ConsensusChessNode.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConsensusChessNode.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConsensusChessNode.dll"]
