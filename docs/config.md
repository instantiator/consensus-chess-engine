# Config

## Database

The `environments` directory should contain database config per environment.

This file is provided to each node and the database in `compose.yaml` and `compose.<env>.yaml` per environment (for `int` and `prod` environments).

| Environment | File | Description |
|-|-|-|
| `int` | `int-database.env` | All environment variables for the integration environment database. |
| `prod` | `prod-database.env` | All environment variables for the production environment database. |

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

There should be a different port per environment.
These are entirely separate postgres instances that ought to be able to run concurrently on testing machines.

In reality, the production database ought to run on a dedicated machine.

## Nodes

The `environments` directory should also contain a number of environment files that give each node its identity.

These environment files are provided to each node in `compose.yaml` and `compose.<env>.yaml` (for `int` and `prod` environments).

At a minimum, there should be config for the engine and a single node in each environment, and in addition there should also be config for the integration tester in the `int` environment.

The filenames provided here follow a simple naming convention - but can be configured per node.

| Environment | File | Description |
|-|-|-|
| `int` | `int-engine.env` | Environment variables for the engine in the integration environment. |
| `int` | `int-node-000-test.env` | Environment variables for a test node in the integration environment. |
| `int` | `int-integration-tester.env` | Config for the integration test runner itself (which also needs an identity to be able to interact with social networks when exercising the nodes). |
| `prod` | `prod-engine.env` | Environment variables for the engine in production. |
| `prod` | `prod-node-001-something.env` | Environment variables for a node in production. |

### File structure

Duplicate and fill out this template:

```env
NODE_NAME=<something helpful>
NODE_SHORTCODE=<unique shortcode>
NETWORK_TYPE=Mastodon
NETWORK_SERVER=<mastodon server domain>
NETWORK_APP_NAME=<name of the app>
NETWORK_APP_KEY=<app key>
NETWORK_APP_SECRET=<app secret>
NETWORK_ACCESS_TOKEN=<app access token>
NETWORK_AUTHORISED_ACCOUNTS=<authorised accounts>
NETWORK_DRY_RUNS=<true|false>
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

### `NETWORK_APP_*`, `NETWORK_ACCESS_TOKEN`

* To connect a node to Mastodon, first register an app inside the developer settings in your account.
* Provide an app name - this is included in the config to help you track multiple apps in an account.
* The app will need `read` and `write` permissions.
* Mastodon will then provide you with an app key, secret and access token, which you can include.

### `NETWORK_AUTHORISED_ACCOUNTS`

* Authorised accounts should contain a comma-separated list of the Mastodon handles for accounts that can issue authorised commands
  * NB. nodes should include the engine in their authorised accounts, the engine should include you
  * eg. `@instantiator@mastodon.social,@icgames@botsin.space`

### `NETWORK_DRY_RUNS`

* Set to `true` to prevent posting to social networks. You'll still see logs about the intention to post. (Useful for testing!)
