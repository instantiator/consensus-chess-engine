# Initialise the database

## Install the Entity Framework tool

```shell
dotnet tool install --global dotnet-ef
```

## Create the initial migration

```shell
dotnet ef migrations add InitialCreate --project ConsensusChessShared/ConsensusChessShared.csproj
```

