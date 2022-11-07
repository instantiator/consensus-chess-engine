#!/bin/bash

set -e
set -o pipefail

# print config and then start the containers
docker compose --env-file environments/database.env config
docker compose --env-file environments/database.env start database

