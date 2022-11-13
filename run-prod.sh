#!/bin/bash

set -e
set -o pipefail

# build containers
docker compose -p consensus-chess-prod \
  -f compose.yaml -f compose.prod.yaml \
  --env-file environments/prod-database.env \
  build

# start the containers
docker compose -p consensus-chess-prod \
  -f compose.yaml -f compose.prod.yaml \
  --env-file environments/prod-database.env \
  up
