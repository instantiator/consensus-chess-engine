# Database

A single postgres database instance serves all nodes in an environment (eg. `int` or `prod`).

Each node and the database itself are configured with a single environment file found in the `environments` directory. See: [config](config.md)

## Entity Framework

This project uses the Entity Framework (EF) as an ORM layer to help manage access to the database. In addition, EF also manages database migrations (where the model has changed).

On each launch of the application, the engine node will run all new migrations against the database before it reports itself healthy - and so all nodes benefit from an updated database.

Some entity framework commands to help you out are included below...

### Prerequisites

First, install the Entity Framework tool:

```shell
dotnet tool install --global dotnet-ef
```

### Create the initial migration

Use this invocation to create a new migration:

```shell
dotnet ef migrations add MyFancyNewMigration --project ConsensusChessShared/ConsensusChessShared.csproj
```
