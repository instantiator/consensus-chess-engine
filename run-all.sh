#!/bin/bash

set -e
set -o pipefail

# build containers
docker compose --env-file environments/database.env build

# print config and then start the containers
docker compose --env-file environments/database.env config
docker compose --env-file environments/database.env up

