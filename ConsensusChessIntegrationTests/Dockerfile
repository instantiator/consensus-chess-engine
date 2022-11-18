FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["ConsensusChessShared/ConsensusChessShared.csproj", "ConsensusChessShared/"]
COPY ["ConsensusChessIntegrationTests/ConsensusChessIntegrationTests.csproj", "ConsensusChessIntegrationTests/"]

RUN dotnet restore ConsensusChessShared/ConsensusChessShared.csproj
RUN dotnet restore ConsensusChessIntegrationTests/ConsensusChessIntegrationTests.csproj

COPY . .

RUN dotnet build ConsensusChessIntegrationTests/ConsensusChessIntegrationTests.csproj
ENTRYPOINT ["dotnet", "test", "ConsensusChessIntegrationTests/ConsensusChessIntegrationTests.csproj", "-l:\"console;verbosity=normal\""]
