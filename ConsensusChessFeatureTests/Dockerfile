FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["ConsensusChessShared/ConsensusChessShared.csproj", "ConsensusChessShared/"]
COPY ["ConsensusChessNode/ConsensusChessNode.csproj", "ConsensusChessNode/"]
COPY ["ConsensusChessEngine/ConsensusChessEngine.csproj", "ConsensusChessEngine/"]
COPY ["ConsensusChessSharedTests/ConsensusChessSharedTests.csproj", "ConsensusChessSharedTests/"]
COPY ["ConsensusChessFeatureTests/ConsensusChessFeatureTests.csproj", "ConsensusChessFeatureTests/"]

RUN dotnet restore ConsensusChessShared/ConsensusChessShared.csproj
RUN dotnet restore ConsensusChessNode/ConsensusChessNode.csproj
RUN dotnet restore ConsensusChessEngine/ConsensusChessEngine.csproj
RUN dotnet restore ConsensusChessSharedTests/ConsensusChessSharedTests.csproj
RUN dotnet restore ConsensusChessFeatureTests/ConsensusChessFeatureTests.csproj

COPY . .

WORKDIR "/src"
RUN dotnet test ConsensusChessSharedTests/ConsensusChessSharedTests.csproj

RUN dotnet build ConsensusChessFeatureTests/ConsensusChessFeatureTests.csproj
ENTRYPOINT ["dotnet", "test", "ConsensusChessFeatureTests/ConsensusChessFeatureTests.csproj", "-l:\"console;verbosity=normal\""]
