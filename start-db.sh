#!/bin/bash

set -e
set -o pipefail

# print config
# docker compose --env-file environments/database.env config

# start the containers
docker compose --env-file environments/database.env start database

