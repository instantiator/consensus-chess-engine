# consensus-chess-engine

A new way to play distributed, consensus games across federated social networks.

_NB. This is a work in progress - the engine is not yet ready to host games._

## Documentation

* [architecture](docs/architecture.md)
* [accounts](docs/accounts.md)
* [config](docs/config.md)
* [database](docs/database.md)
* [commands](docs/commands.md)

### Roadmap

* [Consensus chess 2022](https://trello.com/b/r0OX2iCq/consensus-chess-2022) (Trello board)

## Prerequisites

* [Docker](https://www.docker.com/products/docker-desktop/)

## Getting started

1. Set up some Mastodon accounts, and developer apps for each account, as described in: [accounts](docs/accounts.md)
2. Add config files to `environments` directory, as described in: [config](docs/config.md)
  * for each engine and node
  * for the integration testing service
  * For each database instance
3. Launch and run integration tests with: `./integration-tests.sh`

## Scripts

The following scripts exist for common operations:

| Script | Description |
|-|-|
| `erase-all-docker-artefacts.sh` | Use carefully! This clears down your docker instance - erasing all containers and volumes. |
| `integration-tests.sh` | Launches the db, engine, a node, and the integration test runner for the `int` environment. |
| `run-prod.sh` | Launches the db, engine and all nodes defined for the `prod` environment. |
| `start-db-prod.sh` | Starts the `prod` environment db (only). |
| `stop-all.sh` | Stop all running containers in any environment. |
