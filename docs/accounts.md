# Accounts

The basic configuration needs a number of different sets of account details, provided in config files. See: [config](config.md)

There should ideally be an account per:

* the engine for each environment
* all nodes for each environment
* the integration testing service for the integration environment

## Creating accounts

* Create an account with the Mastodon server of your choice

### API access

* Enter the developer settings, and create a new application
* Fill out a few details, and get hold of:
  * the app key
  * the app secret
  * the access token

You'll need these to create the config file for the engine/node/service that will use this account to interact with Mastodon. See: [config](config.md)
