# Config

## Database

The `environments` directory should contain database config per environment.

This file is provided to each node and the database in `compose.yaml` and `compose.<env>.yaml` per environment (for `int` and `prod` environments).

| Environment | File | Description |
|-|-|-|
| `int` | `db-int.env` | All environment variables for the integration environment database. |
| `prod` | `db-prod.env` | All environment variables for the production environment database. |

### File structure

Duplicate and fill out this template:

```env
DB_HOST=database
PGUSER=<username>
POSTGRES_USER=<username>
POSTGRES_PASSWORD=<password>
POSTGRES_DB=<database-name>
DB_PORT=<database-port>
```

### `DB_HOST`

This is the name of the service, defined in `config.yaml` that contains the database (postgres).

### `PGUSER`, `POSTGRES_USER`, `POSTGRES_PASSWORD`

* `PGUSER` and `POSTGRES_USER` should contain a user you want to create and use to access the database.
* If the user doesn't exist, it will be created.
* `POSTGRES_PASSWORD` is the password for that user.

### `POSTGRES_DB`

This is the name to give the database itself.

* If the database doesn't exist, it will be created by postgres.
* If the model isn't up to date, the latest migrations will be applied by the engine.

### `DB_PORT`

This is the port that the database will run at, and be exposed on.

There should be a different port per environment. These are entirely separate postgres instances that ought to be able to run concurrently.

In reality, the production database should run on a dedicated machine (or vm), but setting the port allows you to test different configurations concurrently on your dev machine (should you need to).

NB. The default port for postgres is `5432` - you _may_ wish to set one of your instances to use this for the sake of simplicity, but it's not required.

## Nodes/Engines

The `environments` directory should also contain a number of environment files that give each node its identity.

These environment files are provided to each node in `compose.yaml` and `compose.<env>.yaml` (for `int` and `prod` environments).

At a minimum, there should be config for the engine and a single node in each environment, and in addition there should also be config for the integration tester in the `int` environment.

The filenames provided here follow a simple naming convention - but can be configured per node.

| Environment | File | Description |
|-|-|-|
| `int` | `engine-int.env` | Environment variables for the engine in the integration environment. |
| `int` | `node-int-00.env` | Environment variables for a test node in the integration environment. |
| `int` | `test-agent-int.env` | Config for the integration test runner itself (which also needs an identity to be able to interact with social networks when exercising the nodes). |
| `prod` | `engine-prod.env` | Environment variables for the engine in production. |
| `prod` | `node-prod-00.env` | Environment variables for a node in production. |

### File structure

Duplicate and fill out this template:

```env
NODE_NAME=<something helpful>
NODE_SHORTCODE=<unique shortcode>
NETWORK_TYPE=Mastodon
NETWORK_SERVER=<mastodon server domain>
NETWORK_APP_NAME_REMINDER=<name of the app>
NETWORK_ACCESS_TOKEN=<app access token>
NETWORK_AUTHORISED_ACCOUNTS=<authorised accounts>
NETWORK_DRY_RUNS=<true|false>
POST_GAME_TAG=<#hashtag>
POST_ADMIN_CONTACT=<contact account>
POST_PUBLIC_VISIBILITY=<Public|Unlisted|Private>
```

Additionally, the integration tester needs a few more pointers so it knows where the engine and node is to bother:

```env
INT_ENGINE_ACCOUNT=<account>
INT_NODE_ACCOUNT<account>
```

### `NODE_NAME`

* Node name should be helpful, human readable, and distinctive.
* It's how the node will describe itself in messages.
* It _needn't_ be unique - this is what the shortcode is for.

### `NODE_SHORTCODE`

Shortcodes are used to refer to participating nodes when setting up games. They are unique idenitifers for nodes.

* Node shortcode must be unique.

### `NETWORK_TYPE`, `NETWORK_SERVER`

* Only 1 network type is supported at the moment: `Mastodon` (capitalisation important, it's matched to an enum)
* Provide the domain of the mastodon server this node belongs to, eg. `botsin.space`

### `NETWORK_ACCESS_TOKEN`

* To connect a node to Mastodon, first register an app inside the developer settings in your account.
* The app will need `read` and `write` permissions.
* Mastodon will then provide you with an access token, which you can include.

### `NETWORK_APP_NAME_REMINDER`

* Provide the app name from your develpoer settings - this is included in the config to help you track/distinguish multiple apps in an account (if, say, you're using the account for multiple services).

### `NETWORK_AUTHORISED_ACCOUNTS`

* Authorised accounts should contain a comma-separated list of the Mastodon handles for accounts that can issue authorised commands
  * NB. nodes should include the engine in their authorised accounts, the engine should include you (and any other administrators)
  * eg. `@instantiator@mastodon.social,@icgames@botsin.space`

### `NETWORK_DRY_RUNS`

* Set to `true` to prevent posting to social networks. You'll still see logs about the intention to post. (Useful for testing!)

### `INT_ENGINE_ACCOUNT`, `INT_NODE_ACCOUNT`

* These are for the integration tester only - as it needs to know which accounts to poke and prod at.
* Provide the account name for your integration testing engine and node (eg. `@icgames_engine@botsin.space`, or `@icgames@botsin.space`)

### `POST_GAME_TAG`

* This is the hashtag to include in game posts, for discoverability - and so it's easy for players to participate in games and conversations.

### `POST_ADMIN_CONTACT`

* The admin contact account, eg. `instantiator@mastodon.social` - for occasional distribution in some posts.

### `POST_PUBLIC_VISIBILITY`

* The visibility for announcement posts. Choose from: `Public`, `Unlisted`, `Private`
