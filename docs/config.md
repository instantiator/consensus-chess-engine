# Config

## Environments

The `environments` directory contains an number of environment files that give each node its identity.

```env
NODE_NAME=<something distinctive>
NODE_SHORTCODE=<unique shortcode>
NETWORK_TYPE=Mastodon
NETWORK_SERVER=<mastodon server domain>
NETWORK_APP_NAME=<name of the app>
NETWORK_APP_KEY=<app key>
NETWORK_APP_SECRET=<app secret>
NETWORK_ACCESS_TOKEN=<app access token>
AUTHORISED_ACCOUNTS=<authorised accounts>
```

### `NODE_NAME`

* Node name should be distinctive, as it's how the node will describe itself in messages - but _needn't_ be unique.

### `NODE_SHORTCODE`

Shortcodes are used to refer to participating nodes when setting up games. They are unique idenitifers for nodes.

* Node shortcode must be unique.

### `NETWORK_TYPE`, `NETWORK_SERVER`

* Only 1 network type is supported at the moment: `Mastodon` (capitalisation important, it's matched to an enum)
* Provide the domain of the mastodon server this node belongs to, eg. `botsin.space`

### `NETWORK_APP_*`

* To connect a node to Mastodon, first register an app inside the developer settings in your account.
* Provide an app name - this is included in the config to help you track multiple apps in an account.
* The app will need `read` and `write` permissions.
* Mastodon will then provide you with an app key, secret and access token, which you can include.

### `AUTHORISED_ACCOUNTS`

* Authorised accounts should contain a comma-separated list of the Mastodon handles for accounts that can issue authorised commands
  * NB. nodes should include the engine in their authorised accounts, the engine should include you
  * eg. `@instantiator@mastodon.social,@icgames@botsin.space`
