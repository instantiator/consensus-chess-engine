# Commands

Issue commands to the engine, or a node, by mentioning it.

You'll need to have administrator permissions for most commands. This is configured in the environment for each node.

## Node

| Keyword | Intent |
|-|-|
| `shutdown` | Shutdown the engine or node. |

## Engine

Issue commands to the engine by mentioning it.

| Keyword | Intent |
|-|-|
| `shutdown` | Shutdown the engine or node. |
| `new` | Creates and starts a new game. |
| `status` | Reports on status. |
| `abandon` | Abandons a game. |
| `advance` | Advances a game. |

## shutdown

eg. `@icgames_engine@botsin.space shutdown`

_NB. As docker compose is currently configured to restart containers on error, and because the shutdown command causes an exception during shutdown, this command is currently ineffectual. The node or engine will restart immediately._

## new

Initiates a new game, all named nodes will participate.

```text
new MoveLock <title> <description> <shortcode> [<shortcode>] [<shortcode>]...
```

eg. `@icgames_engine@botsin.space new MoveLock "First" "The very first consensus chess game." node-prod-00`

## status

Reports on the general status (nodes and games) or a specific game.

```text
status [<shortcode>]
```

eg. `@icgames_engine@botsin.space status`

eg. `@icgames_engine@botsin.space status A6B42J`

## abandon

Abandons a game.

```text
abandon <shortcode>
```

eg. `@icgames_engine@botsin.space abandon A6B42J`

## advance

Advances a game (even if its current move has not expired).

```text
advance <shortcode>
```

eg. `@icgames_engine@botsin.space advance A6B42J`
