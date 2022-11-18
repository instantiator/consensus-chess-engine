# Testing

Scripts support the deployment and execution of feature and integrations tests.

| Script | Description |
|-|-|
| `feature-tests.sh` | Launches the feature tests in a single container, with supporting sqlite db and mocked social network connections. Runs the unit tests during build. |
| `integration-tests.sh` | Launches the db, engine, a node, and the integration test runner for the `int` environment. Runs the unit and feature tests during build. |

It is also possible to run the unit and feature tests locally (without containers) - through Visual Studio, or the `dotnet test` command (examples below).

## Unit tests

**Unit tests are extremely fast - they're testing individual methods and have no dependencies.**

Unit tests are defined in the `ConsensusChessSharedTests` project.

The tests can be run manually through the terminal, or Visual Studio:

```shell
dotnet test ConsensusChessSharedTests/ConsensusChessSharedTests.csproj
```

Both `ConsensusChessEngine/Dockerfile` and `ConsensusChessNode/Dockerfile` also run the unit tests during build.

If the unit tests fail, the build will fail. This will effectively prevent deployment of a solution with any failing tests.

## Feature tests

**The feature tests are reasonably quick, as they have no external dependencies.**

The feature tests run against their own instances of `ConsensusChessNodeService` and `ConsensusChessEngineService`.

They use a local sqlite database in place of the integration/production Postgres db, and mock out the social connections.

This allows testing of each service's interactions without the dependency on an external social network or database.

Both `ConsensusChessEngine/Dockerfile` and `ConsensusChessNode/Dockerfile` also run the feature tests during build. If the feature tests fail, the build will fail. This will effectively prevent deployment of a solution with any failing tests.

The `feature-tests.sh` script will launch the feature tests in a container, with sqlite db and mocked social connections. If the tests fail, the script will return a non-zero code and visually indicate its state.


## Integration tests

**The integration tests can be slow - as they are dependent on interactions between social networks. Some tests can run for 10-15m, as they must wait for messages to filter between Mastodon servers.**

Integration tests are defined in the `ConsensusChessIntegrationTests` project. These tests require access to an operational integration instance, with a database, engine, and at least 1 node.

It has access to the `int` database, and a social network identity of its own. It can monitor the database, post messages and check for engine and node behaviours.

The `integration-tests.sh` script will launch all integration environment services, and trigger the integration tests. If the tests fail, the script will return a non-zero code and visually indicate its state.

| Fail | Pass |
|-|-|
| ![](images/int-tests-fail.png) | ![](images/int-tests-pass.png) |

## Running selected tests

To filter the tests to run for `integration-tests.sh` or `feature-tests.sh`, provide a filter expression in the `--filter` option (abbreviates to `-f`), eg.

```shell
./integration-tests.sh -f VoteOnGameTwice_VoteSuperceded
```

If you just provide a simple filter, it is interpreted to mean "test method name contains". More complex filters are possible. See:

* [Run selected unit tests](https://learn.microsoft.com/en-us/dotnet/core/testing/selective-unit-tests?pivots=mstest) (.NET documentation)

### Investigating

If something fails, often the logs of individual services will give a clue as to why. These are retained by docker.

The unit tests themselves write to `integration-tests.log` in the `logs-int` volume (mounted at `/logs/` in the integration tests container). It's worth exploring that too - or modifying the tests to add a little more trace to it.
