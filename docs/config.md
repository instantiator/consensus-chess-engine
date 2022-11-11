# Config

## Environments

The `environments` directory contains an number of environment files that give each node its identity.

These environment files are specified for each node in `compose.yaml`.

```env
NODE_NAME=<something distinctive>
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

* Node name should be distinctive, as it's how the node will describe itself in messages - but _needn't_ be unique.

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

## Database

Each node is also provided with information and credentials to access the database.

```env
DB_HOST=database
PGUSER=<username>
POSTGRES_USER=<username>
POSTGRES_PASSWORD=<password>
POSTGRES_DB=<database-name>
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
